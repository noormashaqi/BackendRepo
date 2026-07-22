CREATE TABLE AttendanceLogs
(
    Id BIGINT AUTO_INCREMENT PRIMARY KEY,
    EmployeeId BIGINT NOT NULL,
    LoginTime DATETIME NOT NULL DEFAULT UTC_TIMESTAMP(),
    LogoutTime DATETIME NULL,

    CONSTRAINT FK_AttendanceLogs_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(Id),

    INDEX IX_AttendanceLogs_EmployeeId (EmployeeId),
    INDEX IX_AttendanceLogs_LoginTime (LoginTime)
);