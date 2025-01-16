using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Project.Application.DTOs;
using Project.Application.UseCases.Authentication;
using Project.Application.UseCases.Logout;
using Project.Application.UseCases.RefreshToken;
using Project.Application.UseCases.TelegramBot;
using Project.Domain.Interfaces;
using Project.Domain.Security;
using ServiceAbonents.Dtos;
using System.IdentityModel.Tokens.Jwt;
using Telegram.Bot;
using Telegram.Bot.Requests.Abstractions;
using Telegram.Bot.Types;
using WebAPI.Extentions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBotTelegram _botInputRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRequestClient<TransferForAuthRequestDTO> _client;

        public AuthController(IMediator mediator, IBotTelegram botInputRepository, IHttpContextAccessor httpContextAccessor, IRequestClient<TransferForAuthRequestDTO> client) 
        {
            _mediator = mediator;
            _botInputRepository = botInputRepository ?? throw new ArgumentNullException(nameof(botInputRepository));
            _httpContextAccessor = httpContextAccessor;
            _client = client;
        }
        /// <summary>
        /// Аутентификация пользователя, исползуется номер телефона и код авторизации.
        /// </summary>
        /// <param name="request">Запрос для аутентификации пользователя</param>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API(скоре всего неправильные данные)</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpPost("authentication")]
        public async Task<IActionResult> Authentication([FromBody] AuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            var handler = new JwtSecurityTokenHandler();
            if (response is null) return BadRequest();
            if(response.Data is null) return BadRequest();
            Response.Cookies.Append($"refreshToken_{response.Data.userId.ToString()}", response.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(60),
                Path = "/auth"
            }); 
            return Ok(new { response.Data.AccessToken });
        }
        /// <summary>
        /// Обновление токена доступа с использованием refresh-токена. Необходим id user-а.
        /// </summary>
        /// <param name="userId">Id пользователя, которому нужно обновить accesss-токен</param>
        /// <returns>Body - access-токен, Cookie - refresh-токен</returns>
        /// <response code="200">Токен успешно обновлен. Возвращает новый токен доступа.</response>
        /// <response code="400">Неверные данные или запрос не прошел проверку.</response>
        /// <response code="401">Отсутствует refresh-токен или он недействителен.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDTO userId, CancellationToken cansellation)
        {
            if (!Request.Cookies.TryGetValue($"refreshToken_{userId.UserId}", out var refreshToken))
                return Unauthorized("Refresh token is missing.");
            var request = new RefreshTokenRequest(refreshToken);

            var response = await _mediator.Send(request, cansellation);
            if (response is null) return BadRequest();

            Response.Cookies.Append($"refreshToken_{userId}", response.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                MaxAge = TimeSpan.FromDays(60),
                Path = "/auth"
            });

            return Ok(new { response.Data.AccessToken });
        }

        /// <summary>
        /// Работа с ботом, не нужно для работы с клиентом напрямую.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API(скоре всего неправильные данные)</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpPost("bot")]
        public async Task<IActionResult> Bot(
        [FromBody] Update request,
        [FromHeader(Name = "X-Telegram-Bot-Api-Secret-Token")] string secretToken,
        [FromServices] ITelegramBotClient bot,
        CancellationToken cancellation)
        {
            try
            { 
                if (string.IsNullOrWhiteSpace(secretToken) || secretToken != BotConfiguration.Secrets.SecretToken)
                {
                    return Unauthorized("Invalid secret token");
                }


                var telegramRequest = new TelegramBotRequest(request, secretToken, cancellation);

                var response = await _mediator.Send(telegramRequest, cancellation);
                return Ok("Message.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// <summary>
        /// Ввод телефона для получения кода авторизации
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API(скоре всего неправильные данные)</response>
        /// <response code="404">Пользователя не существует</response>
        /// <response code="500">Ошибка сервера</response>
        [HttpPost("setPhone")]
        public async Task<IActionResult> SetPhone([FromServices] ITelegramBotClient bot, [FromBody] PhoneDTO request, CancellationToken ct)
        {
            try
            {
                var response = await _client.GetResponse<TransferForAuthDto>(new TransferForAuthRequestDTO() { PhoneNumber = request.Phone });

                var userResponseDTO = response.Message;

                if (userResponseDTO is null)
                {
                    return StatusCode(404, "Пользователь не найден");
                }

                var webhookUrl = BotConfiguration.Secrets.BotWebhookUrl.AbsoluteUri;
                await bot.SetWebhook(webhookUrl, allowedUpdates: [], secretToken: BotConfiguration.Secrets.SecretToken, cancellationToken: ct);
                var generateCode = CodeGenerator.GenerateCode().ToString();
                try
                {
                    await _botInputRepository.SavePhoneNumberAsync(request.Phone, generateCode);

                    return Ok();
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Failed to save phone number: {ex.Message}");
                }
            }
            catch 
            {
                return StatusCode(404, $"Пользователь не найден");
            }
        }
        /// <summary>
        /// Выход пользователя из аккаунта. Удаление сессии и refresh-токена.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">Успешное выполнение</response>
        /// <response code="400">Ошибка API(скоре всего неправильные данные)</response>
        /// <response code="500">Ошибка сервера</response>
        [Authorize]
        [HttpPost("logout")]

        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return Unauthorized("User is not authenticated.");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid user ID in token.");

            // Проверяем наличие куки с именем "refreshToken_{userId}"
            string cookieName = $"refreshToken_{userId}";
            if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
                return BadRequest($"No refresh token found for session.");


            await _mediator.Send(new LogoutRequest(userId), cancellationToken);

            Response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/auth"
            });

            return Ok($"Session successfully logged out.");
        }
    }
   
}
