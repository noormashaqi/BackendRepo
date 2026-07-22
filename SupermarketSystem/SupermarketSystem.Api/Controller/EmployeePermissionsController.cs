using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Dtos.Permissions;
using SupermarketSystem.Api.Features.Permission;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeePermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeePermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: /api/employees/{id}/permissions
    [HttpGet("{id:long}/permissions")]
    public async Task<IActionResult> GetPermissions(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetEmployeePermissionsQuery(id),
            cancellationToken);

        if (result is null)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        return Ok(result);
    }

    // PATCH: /api/employees/{id}/permissions
    [HttpPatch("{id:long}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
        long id,
        [FromBody] UpdateEmployeePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateEmployeePermissionsCommand(id, request),
            cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                message = result.ErrorMessage
            });
        }

        return Ok(new
        {
            message = "Employee permissions updated successfully."
        });
    }
}