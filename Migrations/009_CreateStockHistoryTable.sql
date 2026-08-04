CREATE TABLE IF NOT EXISTS StockHistory (
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    QuantityAdded INT NOT NULL,
    EmployeeId BIGINT NOT NULL, 
    Date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT FK_StockHistory_Product 
        FOREIGN KEY (ProductId) 
        REFERENCES Product(Id),

    CONSTRAINT FK_StockHistory_Employee 
        FOREIGN KEY (EmployeeId) 
        REFERENCES Employees(Id)
) ENGINE = InnoDB
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;