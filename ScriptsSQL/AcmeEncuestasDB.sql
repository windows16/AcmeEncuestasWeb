-- ==============================================================================
-- Script de Base de Datos - Sistema de Encuestas Acme
-- Motor: Microsoft SQL Server
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
    -- Actualiza las credenciales de prueba para que coincidan con el usuario mostrado en la pantalla.
    UPDATE dbo.Usuarios
    SET PasswordHash = 'K3xGrd1kuO6iFgYK1QY20+gK7CzLJsfK5Hrbq0zW6x0=',
        PasswordSalt = 'usCwtIDqBXe4xT/68ZTKa8x2ojQBOpFfnUsxuPKKmN4=',
        EsActivo = 1
    WHERE Username = 'admin';
END
GO


