using Dapper;
using MediatR;
using MySqlConnector;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Invoices.Create;

public class CreateInvoiceHandler : IRequestHandler<CreateInvoiceCommand, CreateInvoiceResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CreateInvoiceHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<CreateInvoiceResult> Handle(
        CreateInvoiceCommand request,
        CancellationToken cancellationToken)
    {
        using var connection = (MySqlConnection)_connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1) قفل المنتجات المطلوبة وجلب السعر/الكمية/الحالة الحالية
            var productIds = request.Items.Select(i => i.ProductId).ToList();

            var products = (await connection.QueryAsync<ProductStockDto>(
                new CommandDefinition(
                    @"SELECT Id, Name, SellingPrice, Quantity, IsActive
                      FROM Products
                      WHERE Id IN @Ids
                      FOR UPDATE",
                    new { Ids = productIds },
                    transaction: transaction,
                    cancellationToken: cancellationToken)))
                .ToDictionary(p => p.Id);

            // 2) التحقق من المنتجات
            foreach (var item in request.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                    throw new InvalidOperationException($"Product {item.ProductId} not found.");

                if (!product.IsActive)
                    throw new InvalidOperationException($"Product {item.ProductId} is deactivated and cannot be sold.");

                if (item.Quantity > product.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {item.ProductId}. Available: {product.Quantity}, requested: {item.Quantity}.");
            }

            // 3) إنشاء رقم الفاتورة
            var today = DateTime.UtcNow.Date;
            var todayPrefix = today.ToString("yyyyMMdd");

            var lastSequence = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT MAX(CAST(SUBSTRING_INDEX(InvoiceNumber, '-', -1) AS UNSIGNED))
                      FROM Invoices
                      WHERE Date >= @Today
                        AND Date < @Tomorrow
                      FOR UPDATE",
                    new
                    {
                        Today = today,
                        Tomorrow = today.AddDays(1)
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var nextSequence = (lastSequence ?? 0) + 1;
            var invoiceNumber = $"{todayPrefix}-{nextSequence:D3}";

            // 4) حساب الإجماليات
            decimal totalBeforeDiscount = 0;

            var lineItems = new List<(int ProductId, string Name, decimal Price, int Quantity, decimal LineTotal)>();

            foreach (var item in request.Items)
            {
                var product = products[item.ProductId];

                var lineTotal = product.SellingPrice * item.Quantity;

                totalBeforeDiscount += lineTotal;

                lineItems.Add((
                    item.ProductId,
                    product.Name,
                    product.SellingPrice,
                    item.Quantity,
                    lineTotal));
            }

            var totalAfterDiscount =
                totalBeforeDiscount * (1 - request.DiscountPercentage / 100m);

            // 5) إنشاء الفاتورة
            var invoiceId = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    @"INSERT INTO Invoices
                        (InvoiceNumber,
                         EmployeeId,
                         Date,
                         TotalBeforeDiscount,
                         DiscountPercentage,
                         TotalAfterDiscount,
                         HasReturn)
                      VALUES
                        (@InvoiceNumber,
                         @EmployeeId,
                         @Date,
                         @TotalBeforeDiscount,
                         @DiscountPercentage,
                         @TotalAfterDiscount,
                         FALSE);

                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        InvoiceNumber = invoiceNumber,
                        request.EmployeeId,
                        Date = DateTime.UtcNow,
                        TotalBeforeDiscount = totalBeforeDiscount,
                        request.DiscountPercentage,
                        TotalAfterDiscount = totalAfterDiscount
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 6) إضافة عناصر الفاتورة وتحديث المخزون
            foreach (var line in lineItems)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        @"INSERT INTO InvoiceItems
                            (InvoiceId,
                             ProductId,
                             ProductNameSnapshot,
                             UnitPriceSnapshot,
                             Quantity,
                             LineTotal)
                          VALUES
                            (@InvoiceId,
                             @ProductId,
                             @ProductNameSnapshot,
                             @UnitPriceSnapshot,
                             @Quantity,
                             @LineTotal)",
                        new
                        {
                            InvoiceId = invoiceId,
                            line.ProductId,
                            ProductNameSnapshot = line.Name,
                            UnitPriceSnapshot = line.Price,
                            line.Quantity,
                            line.LineTotal
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        @"UPDATE Products
                          SET Quantity = Quantity - @Qty
                          WHERE Id = @ProductId",
                        new
                        {
                            Qty = line.Quantity,
                            line.ProductId
                        },
                        transaction: transaction,
                        cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);

            return new CreateInvoiceResult(invoiceId, invoiceNumber);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

file record ProductStockDto(
    int Id,
    string Name,
    decimal SellingPrice,
    int Quantity,
    bool IsActive);