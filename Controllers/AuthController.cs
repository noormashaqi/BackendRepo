using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth.Login;
using SupermarketSystem.Api.Features.Auth.Logout;
using SupermarketSystem.Api.Features.Auth.Me;
using SupermarketSystem.Api.Features.Auth.RefreshToken;
using SupermarketSystem.Api.Features.Auth.ResetPassword;
using SupermarketSystem.Api.Features.Auth.SignUp;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("sign-up")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> SignUp([FromBody] SignUpRequestDto request)
    {
        var result = await _mediator.Send(new SignUpCommand
        {
            FullName = request.FullName,
            Username = request.Username,
            Password = request.Password,
            Role = request.Role
        });

        if (!result.Success)
        {
            return result.ErrorCode == "UsernameAlreadyExists"
                ? Conflict(new { message = result.ErrorMessage })
                : BadRequest(new { message = result.ErrorMessage });
        }

        return Created(string.Empty, result.Data);
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> SignIn([FromBody] SignInRequestDto request)
    {
        var result = await _mediator.Send(new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        });

        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var result = await _mediator.Send(new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        });

        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId is null)
            return Unauthorized();

        var result = await _mediator.Send(new LogoutCommand
        {
            EmployeeId = employeeId.Value,
            RefreshToken = request.RefreshToken
        });

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("reset-password")]
    [Authorize]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId is null)
            return Unauthorized();

        var result = await _mediator.Send(new ResetPasswordCommand
        {
            EmployeeId = employeeId.Value,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        });

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        return Ok(new { message = "Password reset successfully." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<MeResponseDto>> Me(CancellationToken cancellationToken)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId is null)
            return Unauthorized();

        var result = await _mediator.Send(new GetMeQuery
        {
            EmployeeId = employeeId.Value
        }, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    private long? GetCurrentEmployeeId()
    {
        var employeeIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);

        return employeeIdClaim is not null && long.TryParse(employeeIdClaim.Value, out var employeeId)
            ? employeeId
            : null;
    }
}
