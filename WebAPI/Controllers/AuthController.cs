using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs;
using Project.Application.UseCases.Authentication;
using Project.Application.UseCases.Create;
using Project.Application.UseCases.RefreshToken;
using Project.Application.UseCases.TelegramBot;
using Project.Domain.Interfaces;
using Project.Domain.Security;
using Telegram.Bot;
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
            if (response is null) return BadRequest();
            Response.Cookies.Append($"refreshToken_{Guid.NewGuid().ToString()}", response.Data.RefreshToken, new CookieOptions
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

        [HttpPost("register")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellation)
        {
            var response = await _mediator.Send(request, cancellation);
            if (response is null) return BadRequest();
            return Ok(response);
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
            var ipAddress = _httpContextAccessor.HttpContext.GetClientIp();
            var generateCode = CodeGenerator.GenerateCode().ToString();
            try
            {
                await _botInputRepository.SavePhoneNumberAsync(request.Phone, ipAddress, generateCode);
                
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to save phone number: {ex.Message}");
            }
        }

    }
   
}
