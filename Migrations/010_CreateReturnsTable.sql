CREATE TABLE IF NOT EXISTS Returns (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    OriginalInvoiceId BIGINT NOT NULL,
    Type ENUM('Exchange', 'PureReturn') NOT NULL,
    ProductId INT NOT NULL,
    QuantityReturned INT NOT NULL,
    NewInvoiceId BIGINT NULL,
    EmployeeId BIGINT NOT NULL,
    Date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Reason VARCHAR(255) NULL,
    CONSTRAINT FK_Returns_OriginalInvoice FOREIGN KEY (OriginalInvoiceId) REFERENCES Invoices(Id),
    CONSTRAINT FK_Returns_NewInvoice FOREIGN KEY (NewInvoiceId) REFERENCES Invoices(Id),
    CONSTRAINT FK_Returns_Product FOREIGN KEY (ProductId) REFERENCES Product(Id),
    CONSTRAINT FK_Returns_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);