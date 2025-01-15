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

        public AuthController(IMediator mediator, IBotTelegram botInputRepository, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _botInputRepository = botInputRepository ?? throw new ArgumentNullException(nameof(botInputRepository));
            _httpContextAccessor = httpContextAccessor;
        }

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
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(60),
                Path = "/auth"
            }); 
            return Ok(new { response.Data.AccessToken });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cansellation)
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return Unauthorized("Refresh token is missing.");
            request = request with { RefreshToken = refreshToken };

            var response = await _mediator.Send(request, cansellation);
            if (response is null) return BadRequest();

            Response.Cookies.Append("refreshToken", response.Data.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                MaxAge = TimeSpan.FromDays(60),
                Path = "/auth"
            });

            return Ok(new { response.Data.AccessToken });
        }


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

        [HttpPost("setPhone")]
        public async Task<IActionResult> SetPhone([FromServices] ITelegramBotClient bot, [FromBody] PhoneDTO request, CancellationToken ct)
        {
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

        [Authorize]
        [HttpPost("logout")]

        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            if (!_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return Unauthorized("User is not authenticated.");

            // Извлекаем ID пользователя из JWT-токена
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "Id");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid user ID in token.");

            // Проверяем наличие куки с именем "refreshToken_{userId}"
            string cookieName = $"refreshToken_{userId}";
            if (!Request.Cookies.TryGetValue(cookieName, out var refreshToken) || string.IsNullOrEmpty(refreshToken))
                return BadRequest($"No refresh token found for session.");



            // Удаляем найденную сессию
            await _mediator.Send(new LogoutRequest(userId), cancellationToken);

            // Удаляем соответствующий refresh-токен из cookies
            Response.Cookies.Delete(cookieName, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/auth"
            });

            return Ok($"Session successfully logged out.");
        }
    }
   
}
