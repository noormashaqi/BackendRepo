using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Common;
using SupermarketSystem.Api.Constants;
using SupermarketSystem.Api.Features.Attendance;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/attendance")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly ISender _sender;

    public AttendanceController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [PermissionRequirement(PermissionKeys.AttendanceView)]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAttendanceQuery(date),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("employee/{employeeId:long}")]
    [PermissionRequirement(PermissionKeys.AttendanceViewEmployee)]
    public async Task<IActionResult> GetAttendanceByEmployee(
        long employeeId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAttendanceByEmployeeQuery(employeeId),
            cancellationToken);

        return Ok(result);
    }
}