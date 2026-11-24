-- ==============================================================================
-- Script de Base de Datos - Sistema de Encuestas Acme
-- Normalización: 3FN (Tercera Forma Normal)
-- Seguridad: Almacenamiento seguro de credenciales con Salt + PBKDF2 (SHA-256)
-- ==============================================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AcmeEncuestasDB')
BEGIN
    CREATE DATABASE AcmeEncuestasDB;
END
GO

USE AcmeEncuestasDB;
GO

-- ==============================================================================
-- Tabla Usuarios
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Usuarios')
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioId INT IDENTITY(1,1) NOT NULL,
        NombreCompleto NVARCHAR(150) NOT NULL,
        Username NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        PasswordHash VARCHAR(128) NOT NULL,
        PasswordSalt VARCHAR(128) NOT NULL,
        EsActivo BIT NOT NULL CONSTRAINT DF_Usuarios_EsActivo DEFAULT (1),
        FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT (SYSDATETIME()),
        UltimoLogin DATETIME2(0) NULL,
        CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (UsuarioId),
        CONSTRAINT UQ_Usuarios_Username UNIQUE NONCLUSTERED (Username),
        CONSTRAINT UQ_Usuarios_Email UNIQUE NONCLUSTERED (Email)
    );
END
GO

-- ------------------------------------------------------------------------------
-- Usuario Inicial de Prueba:
-- Usuario: admin
-- Contraseña temporal: Admin123*
-- ------------------------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Username = 'admin')
BEGIN
    INSERT INTO dbo.Usuarios (NombreCompleto, Username, Email, PasswordHash, PasswordSalt, EsActivo)
    VALUES (
        N'Administrador Acme',
        N'admin',
        N'admin@acme.com',
        'K3xGrd1kuO6iFgYK1QY20+gK7CzLJsfK5Hrbq0zW6x0=', -- Hash valido para Admin123* 
        'usCwtIDqBXe4xT/68ZTKa8x2ojQBOpFfnUsxuPKKmN4=', -- Salt valido de 32 bytes
        1
    );
END
ELSE
BEGIN
    UPDATE dbo.Usuarios
    SET PasswordHash = 'K3xGrd1kuO6iFgYK1QY20+gK7CzLJsfK5Hrbq0zW6x0=',
        PasswordSalt = 'usCwtIDqBXe4xT/68ZTKa8x2ojQBOpFfnUsxuPKKmN4=',
        EsActivo = 1
    WHERE Username = 'admin';
END
GO

-- ==============================================================================
-- Tabla Catalogo: TiposCampo
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'TiposCampo')
BEGIN
    CREATE TABLE dbo.TiposCampo (
        TipoCampoId TINYINT NOT NULL,
        Nombre VARCHAR(20) NOT NULL,
        Descripcion VARCHAR(100) NULL,
        CONSTRAINT PK_TiposCampo PRIMARY KEY CLUSTERED (TipoCampoId)
    );

    INSERT INTO dbo.TiposCampo (TipoCampoId, Nombre, Descripcion)
    VALUES 
        (1, 'Texto', 'Entrada de texto general (alfanumérico)'),
        (2, 'Número', 'Entrada numérica entera o decimal'),
        (3, 'Fecha', 'Entrada de fecha (AAAA-MM-DD)');
END
GO

-- ==============================================================================
-- Tabla Encuestas
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Encuestas')
BEGIN
    CREATE TABLE dbo.Encuestas (
        EncuestaId INT IDENTITY(1,1) NOT NULL,
        UsuarioId INT NOT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        LinkUnico VARCHAR(36) NOT NULL,
        EsActiva BIT NOT NULL CONSTRAINT DF_Encuestas_EsActiva DEFAULT (1),
        FechaCreacion DATETIME2(0) NOT NULL CONSTRAINT DF_Encuestas_FechaCreacion DEFAULT (SYSDATETIME()),
        FechaModificacion DATETIME2(0) NULL,
        CONSTRAINT PK_Encuestas PRIMARY KEY CLUSTERED (EncuestaId),
        CONSTRAINT FK_Encuestas_Usuarios FOREIGN KEY (UsuarioId) 
            REFERENCES dbo.Usuarios (UsuarioId),
        CONSTRAINT UQ_Encuestas_LinkUnico UNIQUE NONCLUSTERED (LinkUnico)
    );

    CREATE NONCLUSTERED INDEX IX_Encuestas_UsuarioId ON dbo.Encuestas (UsuarioId);
END
GO

-- ==============================================================================
-- Tabla CamposEncuesta
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'CamposEncuesta')
BEGIN
    CREATE TABLE dbo.CamposEncuesta (
        CampoId INT IDENTITY(1,1) NOT NULL,
        EncuestaId INT NOT NULL,
        NombreCampo NVARCHAR(100) NOT NULL,
        TituloCampo NVARCHAR(200) NOT NULL,
        EsRequerido BIT NOT NULL CONSTRAINT DF_CamposEncuesta_EsRequerido DEFAULT (0),
        TipoCampoId TINYINT NOT NULL,
        Orden INT NOT NULL CONSTRAINT DF_CamposEncuesta_Orden DEFAULT (1),
        CONSTRAINT PK_CamposEncuesta PRIMARY KEY CLUSTERED (CampoId),
        CONSTRAINT FK_CamposEncuesta_Encuestas FOREIGN KEY (EncuestaId) 
            REFERENCES dbo.Encuestas (EncuestaId) ON DELETE CASCADE,
        CONSTRAINT FK_CamposEncuesta_TiposCampo FOREIGN KEY (TipoCampoId) 
            REFERENCES dbo.TiposCampo (TipoCampoId)
    );

    CREATE NONCLUSTERED INDEX IX_CamposEncuesta_EncuestaId ON dbo.CamposEncuesta (EncuestaId);
END
GO

-- ==============================================================================
-- Tabla RespuestasEncuesta
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'RespuestasEncuesta')
BEGIN
    CREATE TABLE dbo.RespuestasEncuesta (
        RespuestaId INT IDENTITY(1,1) NOT NULL,
        EncuestaId INT NOT NULL,
        FechaRespuesta DATETIME2(0) NOT NULL CONSTRAINT DF_RespuestasEncuesta_FechaRespuesta DEFAULT (SYSDATETIME()),
        IpCliente VARCHAR(45) NULL,
        CONSTRAINT PK_RespuestasEncuesta PRIMARY KEY CLUSTERED (RespuestaId),
        CONSTRAINT FK_RespuestasEncuesta_Encuestas FOREIGN KEY (EncuestaId) 
            REFERENCES dbo.Encuestas (EncuestaId) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_RespuestasEncuesta_EncuestaId ON dbo.RespuestasEncuesta (EncuestaId);
END
GO

-- ==============================================================================
-- Tabla ValoresCampo
-- ==============================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ValoresCampo')
BEGIN
    CREATE TABLE dbo.ValoresCampo (
        ValorId INT IDENTITY(1,1) NOT NULL,
        RespuestaId INT NOT NULL,
        CampoId INT NOT NULL,
        Valor NVARCHAR(MAX) NULL,
        CONSTRAINT PK_ValoresCampo PRIMARY KEY CLUSTERED (ValorId),
        CONSTRAINT FK_ValoresCampo_RespuestasEncuesta FOREIGN KEY (RespuestaId) 
            REFERENCES dbo.RespuestasEncuesta (RespuestaId) ON DELETE CASCADE,
        CONSTRAINT FK_ValoresCampo_CamposEncuesta FOREIGN KEY (CampoId) 
            REFERENCES dbo.CamposEncuesta (CampoId)
    );

    CREATE NONCLUSTERED INDEX IX_ValoresCampo_RespuestaId ON dbo.ValoresCampo (RespuestaId);
    CREATE NONCLUSTERED INDEX IX_ValoresCampo_CampoId ON dbo.ValoresCampo (CampoId);
END
GO
