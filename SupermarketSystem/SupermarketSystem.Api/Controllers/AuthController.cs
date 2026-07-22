using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.DTOs.Auth;
using SupermarketSystem.Api.Features.Auth.Login;
using SupermarketSystem.Api.Features.Auth.Logout;

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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command);

        if (!result.Success)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // JwtBearer بيحط الـ "sub" Claim إما باسمه الأصلي أو كـ ClaimTypes.NameIdentifier
        // حسب إعدادات الـ Mapping، فنتأكد من الاثنين
        var employeeIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirst(ClaimTypes.NameIdentifier);

        if (employeeIdClaim is null || !long.TryParse(employeeIdClaim.Value, out var employeeId))
            return Unauthorized();

        var command = new LogoutCommand { EmployeeId = employeeId };
        var success = await _mediator.Send(command);

        return success
            ? Ok(new { message = "تم تسجيل الخروج بنجاح" })
            : NotFound(new { message = "ما في شفت مفتوح لهاد الموظف" });
    }
}
