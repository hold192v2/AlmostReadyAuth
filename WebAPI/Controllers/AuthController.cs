using MediatR;
using Microsoft.AspNetCore.Mvc;
using Project.Application.UseCases.Authentication;
using Project.Application.UseCases.Create;
using Project.Application.UseCases.RefreshToken;
using WebAPI.Extentions;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("authentication")]
        public async Task<IActionResult> Authentication([FromBody] AuthenticationRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            if (response is null) return BadRequest();
            response.Data.Token = JwtExtention.Generate(response.Data);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cansellation)
        {
            var response = await _mediator.Send(request, cansellation);
            if (response is null) return BadRequest();
            response.Data.Token = JwtExtention.Generate(response.Data);
            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellation)
        {
            var response = await _mediator.Send(request, cancellation);
            if (response is null) return BadRequest();
            return Ok(response);
        }
    }
   
}
