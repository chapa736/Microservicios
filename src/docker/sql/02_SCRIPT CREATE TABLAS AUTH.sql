USE AuthDB

BEGIN TRAN TabAuth

IF OBJECT_ID('Roles') IS NULL
BEGIN
    CREATE TABLE Roles (
    Id INT IDENTITY PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL,
    Descripcion VARCHAR(150),
    Activo BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );

    CREATE UNIQUE INDEX UQ_Roles_Nombre ON Roles(Nombre);

    INSERT INTO Roles(Nombre,Descripcion,Activo)
    VALUES('ADMINISTRADOR','ADMINISTRADOR GENERAL',1),('CLIENTE','CLIENTE',1)
END


IF OBJECT_ID('Users') IS NULL
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY PRIMARY KEY,
        Username VARCHAR(50) NOT NULL,
        Email VARCHAR(100) NOT NULL,
        PasswordHash VARCHAR(255) NOT NULL,
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    );

    CREATE UNIQUE INDEX UQ_Users_Username ON Users(Username);
    CREATE UNIQUE INDEX UQ_Users_Email ON Users(Email);
END

IF OBJECT_ID('UserRoles') IS NULL
BEGIN
    CREATE TABLE UserRoles (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id)
    );
END


IF OBJECT_ID('RefreshTokens') IS NULL
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY PRIMARY KEY,
        UserId INT NOT NULL,
        Token VARCHAR(500) NOT NULL,
        FechaExp DATETIME2 NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        Revocado BIT NOT NULL DEFAULT  0,
        CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id)
    );

    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END

COMMIT TRAN TabAuth

