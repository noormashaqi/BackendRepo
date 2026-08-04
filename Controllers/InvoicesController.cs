using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Features.Invoices.Create;
using SupermarketSystem.Api.Features.Invoices.Read;
using SupermarketSystem.Api.Features.Returns.PureReturn;
using SupermarketSystem.Api.Features.Returns.Exchange;
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
        return CreatedAtAction(nameof(GetInvoiceById), new { id = result.InvoiceId }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoiceById(int id, CancellationToken cancellationToken)
    {
        var invoice = await _mediator.Send(new GetInvoiceByIdQuery(id), cancellationToken);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoices(
        [FromQuery] DateTime? date,
        [FromQuery] int? employeeId,
        [FromQuery] int? productId,
        CancellationToken cancellationToken)
    {
        var invoices = await _mediator.Send(new GetInvoicesQuery(date, employeeId, productId), cancellationToken);
        return Ok(invoices);
    }
    
[HttpPost("{id:long}/return")]
public async Task<IActionResult> PureReturn(
    long id,
    [FromBody] PureReturnRequestBody body,
    CancellationToken cancellationToken)
{
    var command = new PureReturnCommand(id, body.ProductId, body.QuantityReturned, body.EmployeeId, body.Reason);
    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}
[HttpPost("{id:long}/exchange")]
public async Task<IActionResult> Exchange(
    long id,
    [FromBody] ExchangeRequestBody body,
    CancellationToken cancellationToken)
{
    var command = new ExchangeCommand(
        id, body.OldProductId, body.QuantityReturned,
        body.NewProductId, body.NewQuantity, body.EmployeeId, body.Reason);

    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}

}