using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Returns.Exchange;

public class ExchangeHandler : IRequestHandler<ExchangeCommand, ExchangeResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ExchangeHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ExchangeResult> Handle(ExchangeCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1) الفحص التراكمي على الصنف القديم (نفس منطق Pure Return بالضبط)
            var originalQuantity = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT Quantity FROM InvoiceItems
                      WHERE InvoiceId = @InvoiceId AND ProductId = @ProductId
                      FOR UPDATE",
                    new { InvoiceId = request.OriginalInvoiceId, ProductId = request.OldProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (originalQuantity is null)
                throw new InvalidOperationException(
                    $"Product {request.OldProductId} was not sold on invoice {request.OriginalInvoiceId}.");

            var alreadyReturned = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT SUM(QuantityReturned) FROM returns
                      WHERE OriginalInvoiceId = @InvoiceId AND ProductId = @ProductId
                      FOR UPDATE",
                    new { InvoiceId = request.OriginalInvoiceId, ProductId = request.OldProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken)) ?? 0;

            if (alreadyReturned + request.QuantityReturned > originalQuantity)
                throw new InvalidOperationException(
                    $"Return quantity exceeds original sold quantity. Sold: {originalQuantity}, already returned: {alreadyReturned}, requested: {request.QuantityReturned}.");

            // 2) قفل المنتج الجديد والتحقق من توفر مخزونه
            var newProduct = await connection.QuerySingleOrDefaultAsync<NewProductStockDto>(
                new CommandDefinition(
                    "SELECT Id, Name, SellingPrice, Quantity, IsActive FROM product WHERE Id = @Id FOR UPDATE",
                    new { Id = request.NewProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (newProduct is null)
                throw new InvalidOperationException($"Product {request.NewProductId} not found.");

            if (!newProduct.IsActive)
                throw new InvalidOperationException($"Product {request.NewProductId} is deactivated and cannot be sold.");

            if (request.NewQuantity > newProduct.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product {request.NewProductId}. Available: {newProduct.Quantity}, requested: {request.NewQuantity}.");

            // 3) رجوع الصنف القديم للمخزون
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE product SET Quantity = Quantity + @Qty WHERE Id = @ProductId",
                    new { Qty = request.QuantityReturned, ProductId = request.OldProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 4) توليد InvoiceNumber للفاتورة الجديدة (نفس منطق تاريخ + رقم يومي المتفق عليه)
            var today = DateTime.UtcNow.Date;
            var todayPrefix = today.ToString("yyyyMMdd");

            var lastSequence = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT MAX(CAST(SUBSTRING_INDEX(InvoiceNumber, '-', -1) AS UNSIGNED))
                      FROM invoices
                      WHERE Date >= @Today AND Date < @Tomorrow
                      FOR UPDATE",
                    new { Today = today, Tomorrow = today.AddDays(1) },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var nextSequence = (lastSequence ?? 0) + 1;
            var newInvoiceNumber = $"{todayPrefix}-{nextSequence:D3}";

            // 5) إنشاء فاتورة جديدة للصنف البديل فقط (بدون خصم، Exchange ما فيها Discount حسب التحليل)
            var lineTotal = newProduct.SellingPrice * request.NewQuantity;

            var newInvoiceId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    @"INSERT INTO invoices
                        (InvoiceNumber, EmployeeId, Date, TotalBeforeDiscount, DiscountPercentage, TotalAfterDiscount, HasReturn)
                      VALUES
                        (@InvoiceNumber, @EmployeeId, @Date, @Total, 0, @Total, FALSE);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        InvoiceNumber = newInvoiceNumber,
                        request.EmployeeId,
                        Date = DateTime.UtcNow,
                        Total = lineTotal
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 6) إدخال InvoiceItem للفاتورة الجديدة (Snapshot للاسم والسعر)
            await connection.ExecuteAsync(
                new CommandDefinition(
                    @"INSERT INTO InvoiceItems
                        (InvoiceId, ProductId, ProductNameSnapshot, UnitPriceSnapshot, Quantity, LineTotal)
                      VALUES
                        (@InvoiceId, @ProductId, @ProductNameSnapshot, @UnitPriceSnapshot, @Quantity, @LineTotal)",
                    new
                    {
                        InvoiceId = newInvoiceId,
                        ProductId = request.NewProductId,
                        ProductNameSnapshot = newProduct.Name,
                        UnitPriceSnapshot = newProduct.SellingPrice,
                        Quantity = request.NewQuantity,
                        LineTotal = lineTotal
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 7) إنقاص مخزون الصنف الجديد
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE product SET Quantity = Quantity - @Qty WHERE Id = @ProductId",
                    new { Qty = request.NewQuantity, ProductId = request.NewProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 8) إدخال سجل الإرجاع (Type = Exchange، مربوط بالفاتورة الجديدة)
            var returnId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    @"INSERT INTO returns
                        (OriginalInvoiceId, Type, ProductId, QuantityReturned, NewInvoiceId, EmployeeId, Date, Reason)
                      VALUES
                        (@OriginalInvoiceId, 'Exchange', @ProductId, @QuantityReturned, @NewInvoiceId, @EmployeeId, @Date, @Reason);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        request.OriginalInvoiceId,
                        ProductId = request.OldProductId,
                        request.QuantityReturned,
                        NewInvoiceId = newInvoiceId,
                        request.EmployeeId,
                        Date = DateTime.UtcNow,
                        request.Reason
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 9) تعليم الفاتورة الأصلية (بدون أي تعديل آخر عليها)
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE invoices SET HasReturn = 1 WHERE Id = @InvoiceId",
                    new { InvoiceId = request.OriginalInvoiceId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            transaction.Commit();

            return new ExchangeResult(returnId, newInvoiceId, newInvoiceNumber);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

file record NewProductStockDto(int Id, string Name, decimal SellingPrice, int Quantity, bool IsActive);