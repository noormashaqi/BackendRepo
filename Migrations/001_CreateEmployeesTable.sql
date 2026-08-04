CREATE TABLE IF NOT EXISTS Employees
(
    Id BIGINT NOT NULL AUTO_INCREMENT,
    FullName VARCHAR(150) NOT NULL,
    Username VARCHAR(100) NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT PK_Employees PRIMARY KEY (Id),
    CONSTRAINT UQ_Employees_Username UNIQUE (Username),
    CONSTRAINT CK_Employees_Role
        CHECK (Role IN ('Admin', 'Cashier', 'InventoryEmployee'))
) ENGINE = InnoDB
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;