using MediatR;

namespace SupermarketSystem.Api.Features.Invoices.Read;

public record GetInvoiceByIdQuery(int Id) : IRequest<InvoiceDto?>;