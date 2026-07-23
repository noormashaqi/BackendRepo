using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Features.Employees;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET /api/employees
    [HttpGet]
    [ProducesResponseType(
        typeof(IEnumerable<EmployeeResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeResponse>>>
        GetEmployees(
            CancellationToken cancellationToken)
    {
        var employees = await _mediator.Send(
            new GetEmployeesQuery(),
            cancellationToken);

        return Ok(employees);
    }

    // GET /api/employees/{id}
    [HttpGet("{id:long}")]
    [ProducesResponseType(
        typeof(EmployeeResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetEmployeeById(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Employee id must be greater than zero."
            });
        }

        var employee = await _mediator.Send(
            new GetEmployeeByIdQuery(id),
            cancellationToken);

        if (employee is null)
        {
            return NotFound(new
            {
                message = "Employee not found."
            });
        }

        return Ok(employee);
    }

    // POST /api/employees
    [HttpPost]
    [ProducesResponseType(
        typeof(EmployeeResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployee(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        if (!result.Success)
        {
            if (result.ErrorCode == "UsernameAlreadyExists")
            {
                return Conflict(new
                {
                    message = result.Message
                });
            }

            return BadRequest(new
            {
                message = result.Message
            });
        }

        var employee = result.Employee!;

        return CreatedAtAction(
            nameof(GetEmployeeById),
            new { id = employee.Id },
            employee);
    }

    // PUT /api/employees/{id}
    [HttpPut("{id:long}")]
    [ProducesResponseType(
        typeof(EmployeeResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployee(
        long id,
        [FromBody] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        var result = await _mediator.Send(
            command,
            cancellationToken);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "NotFound" => NotFound(new
                {
                    message = result.Message
                }),

                "UsernameAlreadyExists" => Conflict(new
                {
                    message = result.Message
                }),

                _ => BadRequest(new
                {
                    message = result.Message
                })
            };
        }

        return Ok(result.Employee);
    }

    // PATCH /api/employees/{id}/deactivate
    [HttpPatch("{id:long}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivateEmployee(
        long id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeactivateEmployeeCommand(id),
            cancellationToken);

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                "NotFound" => NotFound(new
                {
                    message = result.Message
                }),

                "AlreadyDeactivated" => Conflict(new
                {
                    message = result.Message
                }),

                _ => BadRequest(new
                {
                    message = result.Message
                })
            };
        }

        return Ok(new
        {
            message = result.Message
        });
    }
}