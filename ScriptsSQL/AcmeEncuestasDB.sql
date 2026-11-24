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

-- ==============================================================================
-- dbo.CampoEncuestaType
-- pasa la lista estructurada de campos dinámicos a Sp de forma tipada 
-- ==============================================================================
IF TYPE_ID(N'dbo.CampoEncuestaType') IS NULL
BEGIN
    CREATE TYPE dbo.CampoEncuestaType AS TABLE (
        NombreCampo NVARCHAR(100) NOT NULL,
        TituloCampo NVARCHAR(200) NOT NULL,
        EsRequerido BIT NOT NULL,
        TipoCampoId TINYINT NOT NULL,
        Orden INT NOT NULL
    );
END
GO



-- ==============================================================================
-- dbo.sp_GuardarEncuesta
-- Sp que crea y actualiza una encuesta
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_GuardarEncuesta
    @EncuestaId INT = 0,
    @UsuarioId INT,
    @Nombre NVARCHAR(200),
    @Descripcion NVARCHAR(500) = NULL,
    @LinkUnico VARCHAR(36) = NULL OUTPUT,
    @Campos dbo.CampoEncuestaType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @TieneRespuestas BIT = 0;

        IF (@EncuestaId IS NULL OR @EncuestaId <= 0)
        BEGIN
            IF (@LinkUnico IS NULL OR LTRIM(RTRIM(@LinkUnico)) = '')
            BEGIN
                SET @LinkUnico = CAST(NEWID() AS VARCHAR(36));
            END

            INSERT INTO dbo.Encuestas (UsuarioId, Nombre, Descripcion, LinkUnico, EsActiva, FechaCreacion)
            VALUES (@UsuarioId, LTRIM(RTRIM(@Nombre)), NULLIF(LTRIM(RTRIM(@Descripcion)), ''), @LinkUnico, 1, SYSDATETIME());

            SET @EncuestaId = CAST(SCOPE_IDENTITY() AS INT);
        END
        ELSE
        BEGIN
            IF EXISTS (
                SELECT 1
                FROM dbo.RespuestasEncuesta
                WHERE EncuestaId = @EncuestaId
            )
            BEGIN
                SET @TieneRespuestas = 1;

                IF (
                    (SELECT COUNT(1) FROM dbo.CamposEncuesta WHERE EncuestaId = @EncuestaId) <>
                    (SELECT COUNT(1) FROM @Campos)
                    OR EXISTS (
                        SELECT 1
                        FROM dbo.CamposEncuesta c
                        LEFT JOIN @Campos f ON f.Orden = c.Orden
                        WHERE c.EncuestaId = @EncuestaId
                          AND (
                              f.Orden IS NULL
                              OR c.NombreCampo <> f.NombreCampo
                              OR c.TituloCampo <> f.TituloCampo
                              OR c.EsRequerido <> f.EsRequerido
                              OR c.TipoCampoId <> f.TipoCampoId
                          )
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM @Campos f
                        LEFT JOIN dbo.CamposEncuesta c
                            ON c.EncuestaId = @EncuestaId AND c.Orden = f.Orden
                        WHERE c.CampoId IS NULL
                    )
                )
                BEGIN
                    RAISERROR('No se puede cambiar la estructura de campos de una encuesta que ya tiene respuestas.', 16, 1);
                END
            END

            UPDATE dbo.Encuestas
            SET Nombre = LTRIM(RTRIM(@Nombre)),
                Descripcion = NULLIF(LTRIM(RTRIM(@Descripcion)), ''),
                FechaModificacion = SYSDATETIME()
            WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId;

            IF (@@ROWCOUNT = 0)
            BEGIN
                RAISERROR('La encuesta no existe o no pertenece al usuario especificado.', 16, 1);
            END

            -- Reemplazar campos dinámicos asociados
            IF (@TieneRespuestas = 0)
            BEGIN
                DELETE FROM dbo.CamposEncuesta
                WHERE EncuestaId = @EncuestaId;
            END
        END

        -- Insertar los campos dinámicos 
        IF (@TieneRespuestas = 0)
        BEGIN
            INSERT INTO dbo.CamposEncuesta (EncuestaId, NombreCampo, TituloCampo, EsRequerido, TipoCampoId, Orden)
            SELECT 
                @EncuestaId,
                LTRIM(RTRIM(NombreCampo)),
                LTRIM(RTRIM(TituloCampo)),
                EsRequerido,
                TipoCampoId,
                Orden
            FROM @Campos;
        END

        COMMIT TRANSACTION;

        -- Retornar el ID y el Link resultante
        SELECT @EncuestaId AS EncuestaId, @LinkUnico AS LinkUnico;
    END TRY
    BEGIN CATCH
        IF (XACT_STATE() <> 0)
        BEGIN
            ROLLBACK TRANSACTION;
        END

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ==============================================================================
-- dbo.sp_ObtenerEncuestasPorUsuario
-- Retorna el listado de encuestas activas de un usuario
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerEncuestasPorUsuario
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        e.EncuestaId,
        e.UsuarioId,
        e.Nombre,
        e.Descripcion,
        e.LinkUnico,
        e.EsActiva,
        e.FechaCreacion,
        e.FechaModificacion,
        (SELECT COUNT(1) FROM dbo.CamposEncuesta c WITH (NOLOCK) WHERE c.EncuestaId = e.EncuestaId) AS TotalCampos,
        (SELECT COUNT(1) FROM dbo.RespuestasEncuesta r WITH (NOLOCK) WHERE r.EncuestaId = e.EncuestaId) AS TotalRespuestas
    FROM dbo.Encuestas e WITH (NOLOCK)
    WHERE e.UsuarioId = @UsuarioId AND e.EsActiva = 1
    ORDER BY e.FechaCreacion DESC;
END
GO

-- ==============================================================================
-- dbo.sp_ObtenerEncuestaPorId
-- Retorna el Encabezado de la encuesta y listado de campos dinamicos 
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerEncuestaPorId
    @EncuestaId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Encabezado
    SELECT 
        EncuestaId, 
        UsuarioId, 
        Nombre, 
        Descripcion, 
        LinkUnico, 
        EsActiva, 
        FechaCreacion, 
        FechaModificacion
    FROM dbo.Encuestas WITH (NOLOCK)
    WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId AND EsActiva = 1;

    -- Campos dinamicos
    SELECT 
        c.CampoId, 
        c.EncuestaId, 
        c.NombreCampo, 
        c.TituloCampo, 
        c.EsRequerido, 
        c.TipoCampoId, 
        t.Nombre AS TipoCampoNombre, 
        c.Orden
    FROM dbo.CamposEncuesta c WITH (NOLOCK)
    INNER JOIN dbo.TiposCampo t WITH (NOLOCK) ON c.TipoCampoId = t.TipoCampoId
    WHERE c.EncuestaId = @EncuestaId
      AND EXISTS (
          SELECT 1
          FROM dbo.Encuestas e
          WHERE e.EncuestaId = @EncuestaId AND e.UsuarioId = @UsuarioId
      )
    ORDER BY c.Orden ASC, c.CampoId ASC;
END
GO

-- ==============================================================================
-- dbo.sp_EliminarEncuesta
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_EliminarEncuesta
    @EncuestaId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Encuestas
    SET EsActiva = 0,
        FechaModificacion = SYSDATETIME()
    WHERE EncuestaId = @EncuestaId AND UsuarioId = @UsuarioId;
    SELECT @@ROWCOUNT AS FilasAfectadas;
END
GO

-- ==============================================================================
-- dbo.ValorCampoRespuestaType
-- Tipo estructurado (TVP) para almacenar los valores de cada campo
-- ==============================================================================
IF TYPE_ID(N'dbo.ValorCampoRespuestaType') IS NULL
BEGIN
    CREATE TYPE dbo.ValorCampoRespuestaType AS TABLE (
        CampoId INT NOT NULL,
        Valor NVARCHAR(MAX) NULL
    );
END
GO

-- ==============================================================================
-- dbo.sp_ObtenerEncuestaPorLink
-- Consulta una encuesta y sus campos dinámicos para llenado público
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerEncuestaPorLink
    @LinkUnico VARCHAR(36)
AS
BEGIN
    SET NOCOUNT ON;

    -- Cabecera
    SELECT 
        EncuestaId, 
        UsuarioId, 
        Nombre, 
        Descripcion, 
        LinkUnico, 
        EsActiva, 
        FechaCreacion
    FROM dbo.Encuestas WITH (NOLOCK)
    WHERE LinkUnico = @LinkUnico AND EsActiva = 1;

    -- Campos dinámicos requeridos para llenar
    SELECT 
        c.CampoId, 
        c.EncuestaId, 
        c.NombreCampo, 
        c.TituloCampo, 
        c.EsRequerido, 
        c.TipoCampoId, 
        t.Nombre AS TipoCampoNombre, 
        c.Orden
    FROM dbo.CamposEncuesta c WITH (NOLOCK)
    INNER JOIN dbo.Encuestas e WITH (NOLOCK) ON c.EncuestaId = e.EncuestaId
    INNER JOIN dbo.TiposCampo t WITH (NOLOCK) ON c.TipoCampoId = t.TipoCampoId
    WHERE e.LinkUnico = @LinkUnico AND e.EsActiva = 1
    ORDER BY c.Orden ASC, c.CampoId ASC;
END
GO

-- ==============================================================================
-- dbo.sp_GuardarRespuestaEncuesta
-- Registra una nueva respuesta y los valores de cada campo dinámico (Llenado público)
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_GuardarRespuestaEncuesta
    @LinkUnico VARCHAR(36),
    @IpCliente VARCHAR(45) = NULL,
    @Valores dbo.ValorCampoRespuestaType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF EXISTS (
            SELECT CampoId
            FROM @Valores
            GROUP BY CampoId
            HAVING COUNT(1) > 1
        )
        BEGIN
            RAISERROR('La respuesta contiene campos duplicados.', 16, 1);
        END

        DECLARE @EncuestaId INT;

        SELECT @EncuestaId = EncuestaId
        FROM dbo.Encuestas WITH (NOLOCK)
        WHERE LinkUnico = @LinkUnico AND EsActiva = 1;

        IF (@EncuestaId IS NULL)
        BEGIN
            RAISERROR('La encuesta no existe o ya no se encuentra activa.', 16, 1);
        END

        IF EXISTS (
            SELECT 1
            FROM @Valores v
            LEFT JOIN dbo.CamposEncuesta c ON c.CampoId = v.CampoId AND c.EncuestaId = @EncuestaId
            WHERE c.CampoId IS NULL
        )
        BEGIN
            RAISERROR('La respuesta contiene campos que no pertenecen a esta encuesta.', 16, 1);
        END

        IF EXISTS (
            SELECT 1
            FROM dbo.CamposEncuesta c
            LEFT JOIN @Valores v ON v.CampoId = c.CampoId
            WHERE c.EncuestaId = @EncuestaId
              AND c.EsRequerido = 1
              AND (v.CampoId IS NULL OR NULLIF(LTRIM(RTRIM(v.Valor)), '') IS NULL)
        )
        BEGIN
            RAISERROR('La respuesta no incluye todos los campos obligatorios.', 16, 1);
        END

        IF EXISTS (
            SELECT 1
            FROM @Valores v
            INNER JOIN dbo.CamposEncuesta c ON c.CampoId = v.CampoId AND c.EncuestaId = @EncuestaId
            WHERE NULLIF(LTRIM(RTRIM(v.Valor)), '') IS NOT NULL
              AND (
                  (c.TipoCampoId = 2 AND TRY_CONVERT(DECIMAL(38, 10), v.Valor) IS NULL)
                  OR (c.TipoCampoId = 3 AND TRY_CONVERT(DATE, v.Valor, 23) IS NULL)
              )
        )
        BEGIN
            RAISERROR('Uno o más valores no corresponden al tipo de campo configurado.', 16, 1);
        END

        -- Insertar cabecera de la respuesta
        INSERT INTO dbo.RespuestasEncuesta (EncuestaId, FechaRespuesta, IpCliente)
        VALUES (@EncuestaId, SYSDATETIME(), @IpCliente);

        DECLARE @RespuestaId INT = SCOPE_IDENTITY();

        -- Insertar los valores ingresados para cada campo
        INSERT INTO dbo.ValoresCampo (RespuestaId, CampoId, Valor)
        SELECT 
            @RespuestaId,
            v.CampoId,
            v.Valor
        FROM @Valores v
        INNER JOIN dbo.CamposEncuesta c WITH (NOLOCK) ON v.CampoId = c.CampoId
        WHERE c.EncuestaId = @EncuestaId;

        COMMIT TRANSACTION;

        SELECT @RespuestaId AS RespuestaId;
    END TRY
    BEGIN CATCH
        IF (XACT_STATE() <> 0)
        BEGIN
            ROLLBACK TRANSACTION;
        END

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- ==============================================================================
-- dbo.sp_ObtenerResultadosEncuesta
-- Consulta tabulada de respuestas registradas para el dueño de la encuesta
-- ==============================================================================
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerResultadosEncuesta
    @EncuestaId INT,
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Result Set 1: Resumen de la Encuesta
    SELECT 
        e.EncuestaId,
        e.Nombre,
        e.Descripcion,
        e.LinkUnico,
        e.FechaCreacion,
        (SELECT COUNT(1) FROM dbo.RespuestasEncuesta r WITH (NOLOCK) WHERE r.EncuestaId = e.EncuestaId) AS TotalRespuestas
    FROM dbo.Encuestas e WITH (NOLOCK)
    WHERE e.EncuestaId = @EncuestaId AND e.UsuarioId = @UsuarioId;

    -- Result Set 2: Columnas de Campos configurados
    SELECT 
        c.CampoId,
        c.NombreCampo,
        c.TituloCampo,
        c.TipoCampoId,
        c.Orden
    FROM dbo.CamposEncuesta c WITH (NOLOCK)
    WHERE c.EncuestaId = @EncuestaId
      AND EXISTS (
          SELECT 1
          FROM dbo.Encuestas e
          WHERE e.EncuestaId = @EncuestaId AND e.UsuarioId = @UsuarioId
      )
    ORDER BY c.Orden ASC, c.CampoId ASC;

    -- Result Set 3: Respuestas individuales y sus valores registrados
    SELECT 
        r.RespuestaId,
        r.FechaRespuesta,
        r.IpCliente,
        v.CampoId,
        v.Valor
    FROM dbo.RespuestasEncuesta r WITH (NOLOCK)
    LEFT JOIN dbo.ValoresCampo v WITH (NOLOCK) ON r.RespuestaId = v.RespuestaId
    WHERE r.EncuestaId = @EncuestaId
      AND EXISTS (
          SELECT 1
          FROM dbo.Encuestas e
          WHERE e.EncuestaId = @EncuestaId AND e.UsuarioId = @UsuarioId
      )
    ORDER BY r.FechaRespuesta DESC, r.RespuestaId DESC;
END
GO
