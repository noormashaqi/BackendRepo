using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Common;
using SupermarketSystem.Api.Constants;
using SupermarketSystem.Api.DTOs.Reports;
using SupermarketSystem.Api.Features.Reports;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("sales")]
    [PermissionRequirement(PermissionKeys.ReportsView)]
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

    [HttpGet("employees/{id:long}")]
    [PermissionRequirement(PermissionKeys.ReportsView)]
    public async Task<ActionResult<EmployeeDetailReportDto>> GetEmployeeReport(
        long id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetEmployeeReportQuery(id, fromDate, toDate),
            cancellationToken);

        if (result is null)
            return NotFound(new { message = $"Employee with id {id} not found." });

        return Ok(result);
    }

    [HttpGet("products/{id:int}")]
    [PermissionRequirement(PermissionKeys.ReportsView)]
    public async Task<ActionResult<ProductDetailReportDto>> GetProductReport(
        int id,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetProductReportQuery(id, fromDate, toDate),
            cancellationToken);

        if (result is null)
            return NotFound(new { message = $"Product with id {id} not found." });

        return Ok(result);
    }

    [HttpGet("inventory")]
    [PermissionRequirement(PermissionKeys.ReportsView)]
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
    [PermissionRequirement(PermissionKeys.ReportsView)]
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
    [PermissionRequirement(PermissionKeys.ReportsView)]
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
