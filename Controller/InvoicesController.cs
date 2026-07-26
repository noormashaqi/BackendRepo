using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Features.Invoices.Create;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice(
        [FromBody] CreateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(CreateInvoice), new { id = result.InvoiceId }, result);
    }
}