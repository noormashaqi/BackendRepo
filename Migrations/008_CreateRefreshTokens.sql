CREATE TABLE IF NOT EXISTS RefreshTokens
(
    Id BIGINT NOT NULL AUTO_INCREMENT,
    EmployeeId BIGINT NOT NULL,
    TokenHash VARCHAR(128) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    RevokedAt DATETIME NULL,
    ReplacedByTokenHash VARCHAR(128) NULL,

    CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
    CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_RefreshTokens_Employees
        FOREIGN KEY (EmployeeId)
        REFERENCES Employees(Id)
        ON DELETE CASCADE,

    INDEX IX_RefreshTokens_EmployeeId (EmployeeId),
    INDEX IX_RefreshTokens_ExpiresAt (ExpiresAt)
) ENGINE = InnoDB
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
