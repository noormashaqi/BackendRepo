CREATE TABLE Invoices (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    InvoiceNumber VARCHAR(20) NOT NULL UNIQUE,
    EmployeeId BIGINT NOT NULL,
    Date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TotalBeforeDiscount DECIMAL(10,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    TotalAfterDiscount DECIMAL(10,2) NOT NULL,
    HasReturn BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT FK_Invoices_Employee
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(Id)
);

CREATE TABLE InvoiceItems (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    InvoiceId BIGINT NOT NULL,
    ProductId INT NOT NULL, 
    ProductNameSnapshot VARCHAR(255) NOT NULL,
    UnitPriceSnapshot DECIMAL(10,2) NOT NULL,
    Quantity INT NOT NULL,
    LineTotal DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_InvoiceItems_Invoice
        FOREIGN KEY (InvoiceId)
        REFERENCES Invoices(Id),

    CONSTRAINT FK_InvoiceItems_Product
        FOREIGN KEY (ProductId)
        REFERENCES Product(Id)
);