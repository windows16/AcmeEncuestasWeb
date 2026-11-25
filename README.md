# Plataforma de Encuestas Acme

Aplicación web ASP.NET MVC 5 con C# y SQL Server para administrar encuestas dinámicas, compartir un enlace público y consultar respuestas.

## Funcionalidades

- Login con usuario y contraseña.
- Creación, modificación y eliminación lógica de encuestas y sus campos.
- Campos dinámicos de texto, número y fecha, con indicador obligatorio.
- Enlace público único por encuesta; responder no requiere iniciar sesión.
- Validación de campos y tipos tanto en la aplicación como en el procedimiento almacenado.
- Consulta privada de respuestas, ordenadas por fecha, con sus valores por campo.
- Protección de los formularios POST con token antifalsificación y consultas SQL parametrizadas.

## Diseño

Las entidades se separan en `Usuarios`, `Encuestas`, `CamposEncuesta`, `RespuestasEncuesta` y `ValoresCampo`, con claves foráneas e índices para mantener integridad y facilitar las consultas por usuario, encuesta y respuesta. `TiposCampo` es un catálogo para los tres tipos permitidos.

La aplicación separa Controllers, Services con la logica de negocio y repositorios ADO.NET en Data. 

Para conservar la relación entre resultados históricos y su definición, el procedimiento no permite cambiar los campos de una encuesta que ya tiene respuestas; sí permite actualizar su nombre y descripción. Las encuestas sin respuestas pueden modificar libremente sus campos.

## Seguridad de credenciales

Las contraseñas no se almacenan en texto plano ni se cifran de forma reversible. `PasswordHasher` usa PBKDF2-HMAC-SHA256 con salt aleatorio por usuario y compara hashes en tiempo constante. Esto protege las credenciales. los valores de respuesta se guardan como datos de encuesta en SQL Server y no cuentan con cifrado de campo.

## Configuración y ejecución

1. Abra `ScriptsSQL\AcmeEncuestasDB.sql` en SQL Server Management Studio y ejecute el script en una instancia compatible con SQL Server 2016 SP1 o posterior.
2. Revise la cadena `AcmeEncuestasConnection` en `Web.config`. El valor inicial apunta a `.\` con autenticación integrada; ajuste `Data Source` según su instancia.
3. Abra `AcmeEncuestasWeb.sln` en Visual Studio 2022 o posterior, restaure los paquetes NuGet y ejecute con IIS Express.
4. Inicie sesión en `/Account/Login`.

El script crea un usuario de demostración: `admin` / `Admin123*`. 
