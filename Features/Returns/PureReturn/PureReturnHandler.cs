using Dapper;
using MediatR;
using SupermarketSystem.Api.Interface;

namespace SupermarketSystem.Api.Features.Returns.PureReturn;

public class PureReturnHandler : IRequestHandler<PureReturnCommand, PureReturnResult>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PureReturnHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PureReturnResult> Handle(PureReturnCommand request, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // 1) الكمية الأصلية المباعة من هاد الصنف بهاي الفاتورة تحديدًا
            var originalQuantity = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT Quantity FROM InvoiceItems
                      WHERE InvoiceId = @InvoiceId AND ProductId = @ProductId
                      FOR UPDATE",
                    new { InvoiceId = request.OriginalInvoiceId, request.ProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            if (originalQuantity is null)
                throw new InvalidOperationException(
                    $"Product {request.ProductId} was not sold on invoice {request.OriginalInvoiceId}.");

            // 2) مجموع كل عمليات الإرجاع/التبديل السابقة لنفس الصنف بنفس الفاتورة (فحص تراكمي)
            var alreadyReturned = await connection.ExecuteScalarAsync<int?>(
                new CommandDefinition(
                    @"SELECT SUM(QuantityReturned) FROM returns
                      WHERE OriginalInvoiceId = @InvoiceId AND ProductId = @ProductId
                      FOR UPDATE",
                    new { InvoiceId = request.OriginalInvoiceId, request.ProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken)) ?? 0;

            // 3) التحقق: المجموع التراكمي (سابق + هاد الطلب) ما يتجاوز الكمية الأصلية
            if (alreadyReturned + request.QuantityReturned > originalQuantity)
                throw new InvalidOperationException(
                    $"Return quantity exceeds original sold quantity. Sold: {originalQuantity}, already returned: {alreadyReturned}, requested: {request.QuantityReturned}.");

            // 4) إرجاع الكمية للمخزون
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE product SET Quantity = Quantity + @Qty WHERE Id = @ProductId",
                    new { Qty = request.QuantityReturned, request.ProductId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 5) إدخال سجل الإرجاع
            var returnId = await connection.ExecuteScalarAsync<long>(
                new CommandDefinition(
                    @"INSERT INTO returns
                        (OriginalInvoiceId, Type, ProductId, QuantityReturned, NewInvoiceId, EmployeeId, Date, Reason)
                      VALUES
                        (@OriginalInvoiceId, 'PureReturn', @ProductId, @QuantityReturned, NULL, @EmployeeId, @Date, @Reason);
                      SELECT LAST_INSERT_ID();",
                    new
                    {
                        request.OriginalInvoiceId,
                        request.ProductId,
                        request.QuantityReturned,
                        request.EmployeeId,
                        Date = DateTime.UtcNow,
                        request.Reason
                    },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            // 6) تعليم الفاتورة الأصلية إنه فيها إرجاع (بدون أي تعديل تاني عليها)
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE invoices SET HasReturn = 1 WHERE Id = @InvoiceId",
                    new { InvoiceId = request.OriginalInvoiceId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            transaction.Commit();

            return new PureReturnResult(returnId);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}