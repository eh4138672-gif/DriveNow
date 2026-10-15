using Microsoft.AspNetCore.Mvc;
using MyProject.Core.DTOs;
using MyProject.Core.Interfaces;

namespace MyProject.Controllers
{
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // הרשמה של לקוח חדש. מחזירה token, כך שהלקוח מחובר מיד
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
        {
            var result = await _authService.RegisterAsync(dto, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(dto, ct);
            return result.IsSuccess ? Ok(result.Value) : Failure(result);
        }
    }
}
