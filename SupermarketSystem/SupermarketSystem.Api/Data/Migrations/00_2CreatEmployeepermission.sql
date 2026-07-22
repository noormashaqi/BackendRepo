CREATE TABLE EmployeePermissions
(
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    EmployeeId BIGINT NOT NULL,
    PermissionKey VARCHAR(100) NOT NULL,

    CONSTRAINT FK_EmployeePermissions_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(Id)
        ON DELETE CASCADE,

    CONSTRAINT UQ_EmployeePermission
        UNIQUE (EmployeeId, PermissionKey)
);