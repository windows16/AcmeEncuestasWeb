# Plataforma de Encuestas Acme

Aplicación web ASP.NET MVC 5 con C# y SQL Server para administrar encuestas dinámicas, compartir un enlace público y consultar respuestas.

## levantar el proyecto y acceder

1. En SQL Server Management Studio, ejecute `ScriptsSQL\AcmeEncuestasDB.sql` en una instancia compatible con SQL Server 2016 SP1 o posterior.
2. Verifique que `AcmeEncuestasConnection` en `Web.config` apunte a esa instancia y base de datos. El valor inicial de Data Source usa `.\SQLEXPRESS02` y autenticación integrada; ajustelo si su instancia tiene un nombre diferente.
3. Abra `AcmeEncuestasWeb.sln` con Visual Studio 2022 o posterior y restaure los paquetes NuGet (el visual lo hace automatico).
5. Ejecute el proyecto con IIS Express y, cuando se abra el navegador, se mostrara la pantalla del login `/Account/Login`.
6. Para la cuenta de demostración creada por el script, use las siguientes credenciales:

    Usuario: `admin` Contraseña: `Admin123*`.


### Compilación de C# en IIS Express

El proyecto usa el compilador integrado de .NET Framework 4.8 y ya no requiere `bin\roslyn\csc.exe` ni el paquete `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`. Restaure los paquetes(automatico), recompila la solución y reinicie IIS Express.

Al ejecutar el script también se crean dos encuestas para `admin`: **Demo: satisfacción del servicio**, con 55 respuestas de ejemplo y campos de texto, número y fecha; y **Demo: encuesta sin respuestas**.

## Funcionalidades

- Login con usuario y contraseña.
- Creación, modificación y borrado lógico de encuestas y sus campos.
- Campos dinámicos de texto, número y fecha, con indicador obligatorio.
- Enlace público único por encuesta; responder no requiere iniciar sesión.
- Validación de campos obligatorios e integridad referencial en SQL Server, y validación de tipos en la app antes de cifrar.
- Consulta privada y paginada de respuestas, ordenadas por fecha, con sus valores por campo.
- Búsqueda sobre las respuestas de todas las páginas; la paginación conserva el filtro.
- Protección de los formularios POST con token y consultas SQL parametrizadas.

## Arquitectura y decisiones de diseño

### Modelo de datos y relaciones

la bd esta normalizada hasta la 3FN

cada encuesta tiene registros en `CamposEncuesta` y `RespuestasEncuesta`; los valores enviados se guardan en `ValoresCampo`. `TiposCampo` es un catálogo compartido por los campos configurados. llaves primarias, llaves foráneas y restricciones mantienen las relaciones y evitan duplicar datos en cada respuesta.

Se usa `INT` para identificadores, `TINYINT` para catálogos y versiones, `BIT` para opciones booleanas como el borrado logico, `NVARCHAR` para texto Unicode y `VARCHAR` para datos  como IP y hashes en Base64. 

### Estructura de la app

La aplicación usa el patrón MVC: Controllers para controlar las peticiones, Views muestran las pantallas, Services contienen las reglas de negocio y repositorios ADO.NET en Data consultan a SQL Server. Se mantiene la presentación de las pantallas, las validaciones y las consultas en responsabilidades distintas.

respecto a la optimizacion de codigo, usar este patron hace que corregir fallas y hacer mejoras sea mas sencillo.

al utilizar paginacion en los Resultados de las Encuestas hace que el sistema se estrese menos y sea mas rapido.

### Recorridos y acceso a datos

El flujo principal es el siguiente:

1. El administrador inicia sesión. La aplicación verifica la contraseña comparándola con su hash y salt almacenados.
2. Al crear o editar una encuesta, el servicio organiza sus datos y campos; el repositorio los envía a SQL Server mediante parámetros. La encuesta queda asociada a un enlace público único.
3. La persona abre el enlace sin iniciar sesión. En una llamada parametrizada, SQL Server devuelve la información de la encuesta y sus campos en dos consultas, la vista usa esos campos para construir el formulario.
4. Al enviar el formulario, la app valida los campos requeridos y los tipos. Protege los valores y los envía. SQL Server verifica que los campos pertenezcan a la encuesta y guarda la respuesta y sus valores.
5. El administrador consulta los resultados de una encuesta propia. SQL Server devuelve tres result sets: el resumen, las columnas y los valores paginados de 50 respuestas.

toda la data de la logica de negocio se encuentra en la bd, no se utilizan valores quemados ni constantes.

### Justificación de la estructura de la Base de datos y de aplicación

normalize la base de datos hasta la 3FN ya que en ese punto se resuelven la mayoria de problemas de redundancias e inconsistencias.

cada tabla almacena su tipo de info y así se reduce la duplicidad e inconsistencias en los registros. Usuarios guarda las cuentas, Encuestas el encabezado de la encuesta; CamposEncuesta, los campos que se tiene que llenar en la Encuesta; RespuestasEncuesta, cada envío con el formulario lleno...

En la app, use MVC por ser un estandar y debido a que separa la web, los servicios y el acceso a SQL Server por repos, Esto hace que sea facil corregir errores y hacer mejoras reutilizando codigo, ya que el sistema esta separado en carpetas con sus componentes especificos, buscarlos y entender el software es mas sencillo.

### Cifrado y respuestas

Las contraseñas no se cifran de forma reversible: se verifican con PBKDF2-HMAC-SHA256, salt individual de 32 bytes y una derivación de 32 bytes. 

Las respuestas se protegen en la aplicación antes de enviarse a SQL Server mediante `MachineKey.Protect`. 

### Consulta y búsqueda de datos 

La consulta normal recupera y descifra una página de 50 respuestas, limitando la memoria y la transferencia. La búsqueda abarca todas las páginas, pero como SQL Server no puede buscar en los valores protegidos, la aplicación recorre las páginas y compara los valores despues de descifrarlos.
