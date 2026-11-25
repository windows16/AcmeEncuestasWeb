# Plataforma de Encuestas Acme

Aplicación web ASP.NET MVC 5 con C# y SQL Server para administrar encuestas dinámicas, compartir un enlace público y consultar respuestas.

## Funcionalidades

- Login con usuario y contraseña.
- Creación, modificación y eliminación lógica de encuestas y sus campos.
- Campos dinámicos de texto, número y fecha, con indicador obligatorio.
- Enlace público único por encuesta; responder no requiere iniciar sesión.
- Validación de campos obligatorios e integridad referencial en SQL Server, y validación de tipos en la aplicación antes de cifrar.
- Consulta privada y paginada de respuestas, ordenadas por fecha, con sus valores por campo.
- Protección de los formularios POST con token antifalsificación y consultas SQL parametrizadas.

## Diseño

Las entidades se separan en `Usuarios`, `Encuestas`, `CamposEncuesta`, `RespuestasEncuesta` y `ValoresCampo`, con claves foráneas e índices para mantener integridad y facilitar las consultas por usuario, encuesta y respuesta. `TiposCampo` es un catálogo para los tres tipos permitidos.

La aplicación separa Controllers, Services con la logica de negocio y repositorios ADO.NET en Data. 

Para conservar la relación entre resultados históricos y su definición, el procedimiento no permite cambiar los campos de una encuesta que ya tiene respuestas; sí permite actualizar su nombre y descripción. Las encuestas sin respuestas pueden modificar libremente sus campos.

## Seguridad de datos

Las contraseñas se almacenan como hash PBKDF2-HMAC-SHA256 con salt individual; no se cifran de forma reversible. Los valores de respuesta se protegen con `MachineKey.Protect` de ASP.NET antes de enviarlos a SQL Server y se validan/descifran al preparar una página de resultados. El formato lleva versión para poder evolucionar la protección. La clave no se guarda en la base de datos. En instalaciones de un solo servidor se usan las claves protegidas del entorno ASP.NET; en una granja todos los nodos deben compartir una configuración `machineKey` protegida y administrada fuera del repositorio. Mantenga restringidos los permisos de la identidad IIS y proteja las copias de seguridad antiguas, que podrían contener respuestas sin cifrar.

Los valores históricos en texto plano se cifran gradualmente cuando se consulta su página de resultados, en lotes de hasta 50 respuestas. Los resultados se consultan paginados para limitar memoria y transferencia; el orden es por fecha y un identificador estable.

## Consultas y consistencia

Los procedimientos usan parámetros tipados/TVP y `OFFSET/FETCH` para cargar solo la página solicitada. Se evita `NOLOCK`, que podría devolver lecturas no confirmadas o inconsistentes. El índice de respuestas por encuesta, fecha e identificador soporta la paginación.

## Configuración y ejecución

1. Abra `ScriptsSQL\AcmeEncuestasDB.sql` en SQL Server Management Studio y ejecute el script en una instancia compatible con SQL Server 2016 SP1 o posterior.
2. Revise la cadena `AcmeEncuestasConnection` en `Web.config`. El valor inicial apunta a `.\` con autenticación integrada; ajuste `Data Source` según su instancia.
3. Abra `AcmeEncuestasWeb.sln` en Visual Studio 2022 o posterior, restaure los paquetes NuGet y ejecute con IIS Express.
4. Inicie sesión en `/Account/Login`.

El script crea un usuario de demostración: `admin` / `Admin123*`. 
