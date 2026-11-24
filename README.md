# Sistema de Encuestas Acme

Plataforma web desarrollada en **ASP.NET MVC 5** con **C#** y **Microsoft SQL Server** para la gestión y llenado de encuestas en línea.

---

## Requisito Implementado: Autenticación (Login)

cubre en su totalidad el **Requisito 1: "Login con usuario y password"**, respondiendo directamente a los criterios de evaluación:

### 1. Base de Datos (SQL Server)
* **Normalización (3FN):** La tabla `dbo.Usuarios` almacena información atómica sin redundancias ni dependencias transitivas.
* **Tipos de datos optimizados:**
  * `INT IDENTITY(1,1)` para clave primaria indexada.
  * `NVARCHAR(50)` y `NVARCHAR(100)` con restricciones `UNIQUE` para evitar duplicidad de usuarios y correos.
  * `VARCHAR(128)` para los campos criptográficos (almacenamiento en Base64).
  * `DATETIME2(0)` para un menor consumo de almacenamiento en auditorías de fechas frente al tipo tradicional `DATETIME`.
  * `BIT` con valor por defecto `1` para borrado lógico / estado activo.

### 2. Seguridad de la Información y Criptografía
* **Algoritmo de codificado (Punto 4.1):** Las contraseñas **nunca se guardan en texto plano**. Se implementó **PBKDF2 con HMAC-SHA256** (`Rfc2898DeriveBytes`) y **10,000 iteraciones**, siguiendo las directrices de NIST contra ataques de fuerza bruta y diccionarios.
* **Salting Criptográfico:** Cada usuario cuenta con un **Salt aleatorio de 256 bits** (`RNGCryptoServiceProvider`), neutralizando por completo el uso de Rainbow Tables.
* **Prevención de Timing Attacks (Punto 4.2):** La verificación de contraseñas compara los arreglos de bytes en tiempo constante para evitar ataques de canal lateral (*side-channel timing attacks*).
* **Seguridad Web:** Formulario protegido contra ataques de falsificación de petición en sitios cruzados con `[ValidateAntiForgeryToken]` y cookies de autenticación `FormsAuthentication` con banderas seguras.

### 3. Arquitectura de la Solución
```
AcmeEncuestasWeb/
├── Controllers/
│   └── AccountController.cs          # Controlador MVC: Login, Logout, validación de estado
├── Data/
│   └── UsuarioRepository.cs          # Capa de Acceso a Datos optimizada con ADO.NET parametrizado
├── Models/
│   └── Usuario.cs                    # Entidad de dominio de la tabla Usuarios
├── ViewModels/
│   └── LoginViewModel.cs             # Modelo de vista con DataAnnotations para validación
├── Services/
│   ├── PasswordHasher.cs             # Motor de cifrado seguro PBKDF2 + SHA-256 + Salt
│   └── AuthService.cs                # Orquestador de autenticación y lógica de negocio
├── Views/
│   ├── Account/
│   │   └── Login.cshtml              # Vista Razor con interfaz Bootstrap y feedback visual
│   └── Shared/
│       └── _LoginPartial.cshtml      # Componente de navegación para usuario autenticado
├── ScriptsSQL/
│   └── 01_CreacionBaseDatosYUsuarios.sql  # Script DDL + usuario de prueba
└── Web.config                        # Cadena de conexión y configuración FormsAuthentication
```

---

## Pasos para Probar el Login en Local

### Paso 1: Ejecutar el Script de Base de Datos
1. Abre **SQL Server Management Studio (SSMS)** y conéctate a tu instancia local (`.` o `localhost`).
2. Abre y ejecuta el archivo:
   `AcmeEncuestasWeb\ScriptsSQL\AcmeEncuestasDB.sql`
3. Esto creará la base de datos `AcmeEncuestasDB`, la tabla `Usuarios` y el usuario administrador de prueba.

### Paso 2: Configurar la Cadena de Conexión 
En el archivo `Web.config`, la cadena ya apunta a tu SQL Server local por defecto con autenticación de Windows:
```xml
<connectionStrings>
  <add name="AcmeEncuestasConnection" 
       connectionString="Data Source=.;Initial Catalog=AcmeEncuestasDB;Integrated Security=True;MultipleActiveResultSets=True;" 
       providerName="System.Data.SqlClient" />
</connectionStrings>
```
*(Si utiliza autenticación de SQL Server con usuario `sa`, solo ajusta el `User Id` y `Password`).*

*"Si su instancia local de SQL Server utiliza un nombre específico (ej. .\SQLEXPRESS), únicamente modifique el atributo Data Source=.\SQLEXPRESS en el Web.config."*

### Paso 3: Iniciar la Aplicación
1. Abre la solución `AcmeEncuestasWeb.sln` en **Visual Studio**.
2. Presiona **F5** (o botón *IIS Express* en verde).
3. Navega a `/Account/Login` o haz clic en **"Iniciar Sesión"** en la barra superior.

### Credenciales de Prueba:
* **Usuario:** `admin`
* **Contraseña:** `Admin123*`
