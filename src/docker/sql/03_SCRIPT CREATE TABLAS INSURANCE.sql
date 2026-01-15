USE InsuranceDB

BEGIN TRAN TabInsurance

IF OBJECT_ID('Clientes') IS NULL
BEGIN
    CREATE TABLE Clientes (
        Id INT IDENTITY PRIMARY KEY,
        NumeroIdentificacion VARCHAR(10) NOT NULL,
        Nombre VARCHAR(40) NOT NULL,
        ApPaterno VARCHAR(40) NOT NULL,
        ApMaterno VARCHAR(40) NOT NULL,
        Telefono VARCHAR(20) NOT NULL,
        Email VARCHAR(100) NOT NULL,
        Direccion VARCHAR(250),
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        FechaActualizacion DATETIME2 NULL,
        UserId INT NULL
    );

    ALTER TABLE Clientes
    ADD CONSTRAINT UQ_Clients_Identificacion UNIQUE (NumeroIdentificacion);

    ALTER TABLE Clientes
    ADD CONSTRAINT CK_Clients_Identificacion
    CHECK (LEN(NumeroIdentificacion) = 10 AND NumeroIdentificacion NOT LIKE '%[^0-9]%');

    CREATE NONCLUSTERED INDEX IX_Clientes_Apellidos_Nombre ON dbo.Clientes(ApPaterno, ApMaterno, Nombre);
    CREATE NONCLUSTERED INDEX IX_Clientes_Email ON dbo.Clientes(Email);
END

IF OBJECT_ID('Polizas') IS NULL
BEGIN
    CREATE TABLE Polizas (
        Id INT IDENTITY PRIMARY KEY,
        IdCliente INT NOT NULL,
        TipoPoliza INT NOT NULL,
        FechaInicio DATE NOT NULL,
        FechaFin DATE NOT NULL,
        Monto DECIMAL(18,2) NOT NULL,
        Estatus INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        CONSTRAINT FK_Polizas_Cliente FOREIGN KEY (IdCliente) REFERENCES Clientes(Id)
    );

    CREATE INDEX IX_Polizas_IdCliente ON Polizas(IdCliente);
    CREATE INDEX IX_Polizas_TipoPoliza ON Polizas(TipoPoliza);
    CREATE INDEX IX_Polizas_Estatus ON Polizas(Estatus);
    CREATE INDEX IX_Polizas_Fechas ON Polizas(FechaInicio, FechaFin);

END


COMMIT TRAN TabInsurance
