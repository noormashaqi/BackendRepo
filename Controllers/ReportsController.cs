using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Features.Reports;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? employeeId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetSalesReportQuery(fromDate, toDate, employeeId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> GetInventoryReport(
        [FromQuery] int? categoryId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetInventoryReportQuery(categoryId, activeOnly),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceReportDto>> GetAttendanceReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] long? employeeId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetAttendanceReportQuery(fromDate, toDate, employeeId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("employees")]
    public async Task<ActionResult<EmployeesReportDto>> GetEmployeesReport(
        [FromQuery] bool? activeOnly,
        [FromQuery] string? role,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetEmployeesReportQuery(activeOnly, role),
            cancellationToken);

        return Ok(result);
    }
}
