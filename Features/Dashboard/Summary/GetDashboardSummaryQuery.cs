using MediatR;

namespace SupermarketSystem.Api.Features.Dashboard.Summary;

public record GetDashboardSummaryQuery : IRequest<GetDashboardSummaryResult>;