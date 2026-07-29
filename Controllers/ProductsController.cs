using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupermarketSystem.Api.Services.Products;

namespace SupermarketSystem.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? categoryId, [FromQuery] bool activeOnly = true)
    {
        var result = await _mediator.Send(new GetProductsQuery
        {
            CategoryId = categoryId,
            ActiveOnly = activeOnly
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery { Id = id });

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        await _mediator.Send(new DeactivateProductCommand { Id = id });
        return NoContent();
    }

    [HttpPost("{id}/stock/add")]
    public async Task<IActionResult> AddStock(int id, [FromBody] AddStockCommand command)
    {
        command.ProductId = id;

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}