# Microservicios

![Badge](https://img.shields.io/badge/.NET-6-brightgreen) ![Badge](https://img.shields.io/badge/Architecture-Clean%20Architecture%20%2B%20CQRS-blue) ![Badge](https://img.shields.io/badge/License-See%20repo-lightgrey)

Microservicios para el sistema de gestión de pólizas. Este repositorio contiene dos microservicios principales:

- Auth: microservicio de autenticación (genera JWT).
- Seguros: microservicio para gestión de clientes y pólizas (valida JWT, aplica políticas y soporta cache opcional).

La solución sigue una arquitectura basada en Clean Architecture + CQRS (MediatR) con Dapper para acceso a datos, Serilog para logging y Swagger para documentación API.

---

## Contenido
- src/Auth: microservicio de Autenticación
  - Controllers, Handlers, TokenService, Middleware, configuración de Serilog y JWT.
- src/Seguros: microservicio de Seguros
  - Endpoints para Clientes y Pólizas, validación JWT, políticas y cache opcional (Redis).
- Microservicios.Auth.sln: solución con ambos microservicios.

---

## Features principales
- Generación y validación de JWT.
- Arquitectura Clear + CQRS (MediatR) para separar lectura/escritura.
- Dapper para consultas eficientes y UnitOfWork para transacciones.
- Serilog: logs por consola y ficheros con rotación por tamaño.
- Swagger (Swashbuckle) habilitado en Development / Docker (ruta raíz).
- Middleware para manejo centralizado de excepciones y logging de requests.
- Políticas de autorización en Seguros (Admin/Cliente) y cache opcional (Redis).

---

## Arquitectura y tecnologías principales
- Clean Architecture: Core / Domain / Application / Infrastructure / WebAPI
- CQRS + MediatR (Handlers para Commands y Queries)
- Dapper (acceso a datos)
- JWT (System.IdentityModel.Tokens.Jwt) — Token generation en Auth, token validation en Seguros
- Serilog (console + file rolling)
- Swagger (Swashbuckle) para documentación de API
- BCrypt.Net para hashing de contraseñas (en infraestructura cuando aplica)
- SQL Server (ejemplos de configuración)
- Redis (opcional) — StackExchange.Redis / IDistributedCache
- .NET 6

---

## Requisitos previos
- .NET 6 SDK (dotnet)
- SQL Server (o una instancia compatible)
- (Opcional) Redis si se quiere habilitar la cache distribuida en Seguros
- (Opcional) Visual Studio 2022
- Acceso para escribir en carpeta `Logs` (permiso para generar ficheros de log)

---

## Quick start (local)

Desde la raíz del repositorio:

1. Restaurar y compilar la solución:
   - dotnet build Microservicios.Auth.sln

2. Ejecutar Auth API (en terminal separada):
   - dotnet run --project src/Auth/Auth.WebAPI
   - Swagger estará disponible en la raíz del host si ASPNETCORE_ENVIRONMENT es Development o si la app detecta "Docker" (ej. https://localhost:5001). Ver Program.cs.

3. Ejecutar Seguros API (en otra terminal):
   - dotnet run --project src/Seguros/Seguros.WebAPI
   - Swagger estará disponible en la raíz en Development/Docker.

Nota:
- Ejecuta cada servicio en terminal separada.
- Si necesitas correr ambos en el mismo puerto ajusta launchSettings.json o variables de entorno (ASPNETCORE_URLS o Kestrel settings).
- Swagger se habilita cuando app.Environment.IsDevelopment() o IsEnvironment("Docker").

---

## Configuración (appsettings.json / variables)

Importante: los valores de JwtSettings (SecretKey, Issuer, Audience) deben ser consistentes entre Auth y Seguros para que Seguros pueda validar los tokens emitidos por Auth.

Ejemplo (extraído de src/Auth/Auth.WebAPI/appsettings.json):

```json
{
  "ConnectionStrings": {
    "AuthDB": "Server=.;Database=AuthDB;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "SecretKey": "AuthSecretKey2026!@#$%^&*()_+",
    "Issuer": "Auth.API",
    "Audience": "Seguros.Microservicios",
    "ExpiryInHours": 1
  },
  "TamanhoMaximoLogEnMegas": 10
}
```

Puntos clave:
- Auth: ConnectionStrings key = "AuthDB" (usado en AddAuthServices).
- Seguros: ConnectionStrings que se busca en AddSegurosServices es "InsuranceDB" (ver ServiceCollectionExtensions).
- JwtSettings.SecretKey, Issuer, Audience: deben coincidir entre Auth y Seguros.
- TamanhoMaximoLogEnMegas: controla fileSizeLimitBytes de Serilog.
- Redis: si usas Redis, configura:
  - Redis:ConnectionString
  - Redis:InstanceName (opcional, por defecto "Seguros:")

Variables de entorno útiles:
- ASPNETCORE_ENVIRONMENT (Development/Production/Docker)
- ConnectionStrings__AuthDB
- ConnectionStrings__InsuranceDB
- JwtSettings__SecretKey
- JwtSettings__Issuer
- JwtSettings__Audience
- Redis__ConnectionString
- Redis__InstanceName

Nota de implementación:
- El archivo src/Auth/Auth.Infrastructure/Services/TokenService actualmente genera AccessToken con expiración fija a 1 hora (DateTime.UtcNow.AddHours(1)). Hay una clave ExpiryInHours en appsettings; si deseas que el TokenService use ese valor, sincronízalo en la implementación del servicio.

---

## Autenticación: endpoints principales (Auth)

Rutas implementadas en src/Auth/Auth.WebAPI/Controllers/AuthController.cs:

- POST /api/auth/login
- POST /api/auth/register
- POST /api/auth/refresh
- POST /api/auth/logout
- GET /health (health checks)

Ejemplo payload login:

```json
// POST /api/auth/login
{
  "username": "usuario@example.com",
  "password": "MiPassword123"
}
```

Respuesta exitosa (BaseResponse<LoginResponseDto>):
- Contiene AccessToken (JWT) y RefreshToken (si está implementado en handlers).

Ejemplos curl:

1) Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"password"}'
```

2) Register

```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"nuevo","email":"nuevo@ej.com","password":"Pass123!","roleId":2}'
```

3) Refresh token

```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '"<refreshToken>"'
```

4) Logout

```bash
curl -X POST https://localhost:5001/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '"<refreshToken>"'
```

---

## Uso del token en otros microservicios (ej. Seguros)

- Después de login obtén el AccessToken (JWT).
- En peticiones a Seguros añade header:
  Authorization: Bearer <ACCESS_TOKEN>

Ejemplo:

```bash
curl -X GET https://localhost:5001/api/clientes \
  -H "Authorization: Bearer <ACCESS_TOKEN>"
```

Consideraciones:
- Seguros define políticas y roles (ej. ADMINISTRADOR, CLIENTE). Asegúrate de que el JWT incluya roles (TokenService añade ClaimTypes.Role por cada rol en UserDto.Roles).
- Seguros registra eventos de JwtBearer para diagnóstico (OnAuthenticationFailed, OnTokenValidated, OnChallenge, OnMessageReceived).

---

## Endpoints principales - Seguros (resumen)

Controladores y rutas principales (ver src/Seguros/Seguros.WebAPI/Controllers):

ClientesController (ruta base: /api/clientes):
- GET /api/clientes (ADMINISTRADOR)
- GET /api/clientes/{id}
- GET /api/clientes/User/{userId}
- GET /api/clientes/identificacion/{numeroIdentificacion} (ADMINISTRADOR)
- POST /api/clientes (ADMINISTRADOR)
- PUT /api/clientes/{id} (ADMINISTRADOR)
- PATCH /api/clientes/UpdateMyInfo (CLIENTE) — editar información personal
- DELETE /api/clientes/{id} (ADMINISTRADOR)

PolizasController (ruta base: /api/polizas):
- GET /api/polizas/{id}
- GET /api/polizas/cliente/{clienteId}
- GET /api/polizas/tipo/{tipo} (ADMINISTRADOR)
- GET /api/polizas/estatus/{estatus} (ADMINISTRADOR)
- GET /api/polizas/vigentes (ADMINISTRADOR)
- POST /api/polizas (ADMINISTRADOR)
- POST /api/polizas/{id}/cancelar (CLIENTE)

Autorización y políticas:
- En AddSegurosServices se registran políticas:
  - "AdminOnly" => RequireRole("ADMINISTRADOR")
  - "ClienteOnly" => RequireRole("CLIENTE")
  - "AdminOrCliente" => RequireRole("ADMINISTRADOR", "CLIENTE")

---

## Logging & Health checks
- Serilog configurado en cada microservicio para consola y fichero:
  - Logs/Auth-YYYYMMDD.txt
  - Logs/Seguros-YYYYMMDD.txt
- Tamaño máximo de fichero controlado por `TamanhoMaximoLogEnMegas` (appsettings).
- Health checks expuestos en `/health` (MapHealthChecks) en ambos servicios.
- Los pipelines incluyen:
  - app.UseSerilogRequestLogging();
  - RequestLoggingMiddleware (logs de inicio/fin de request)
  - ExceptionHandlingMiddleware (respuestas uniformes en JSON con BaseResponse)

---

## Middleware y cross-cutting concerns
- ExceptionHandlingMiddleware: centraliza manejo de excepciones y devuelve BaseResponse con errores y códigos HTTP adecuados (Unauthorized, NotFound, BadRequest, InternalServerError).
- RequestLoggingMiddleware: logs de inicio/fin de request con duración.
- JwtBearer Events: eventos implementados para diagnóstico en OnAuthenticationFailed, OnTokenValidated, OnChallenge y OnMessageReceived.

---

## Cache (Seguros)
- Seguros puede usar Redis si configuras `Redis:ConnectionString`:
  - Se registra IDistributedCache con StackExchangeRedis.
  - Se registra ConnectionMultiplexer si se necesita acceso directo.
- Si Redis no está configurado, Seguros usa AddDistributedMemoryCache() como fallback.

---

## Desarrollo: cómo agregar nuevas funcionalidades

Commands (escritura)
- Crear Command en Core/Commands
- (Opcional) DTO en Core/DTOs
- Crear CommandHandler en Application/Handlers
- Agregar método al repositorio (Infrastructure) si es necesario
- Agregar endpoint en Controller
- Probar con Swagger

Queries (lectura)
- Crear Query en Core/Queries
- Crear QueryHandler en Application/Handlers
- Agregar método optimizado Dapper en repositorio
- Agregar endpoint en Controller
- Probar con Swagger

Buenas prácticas:
- Handlers retornan BaseResponse<T>
- Validación (FluentValidation o DataAnnotations)
- Logging con ILogger para handlers críticos
- UnitOfWork / transacciones para operaciones compuestas

---

## Tests
- Si existen proyectos de pruebas: dotnet test <TestProject.csproj> o dotnet test en la solución.
- Actualmente la solución no incluye proyectos de test en la raíz; se recomienda añadir proyectos de test por servicio (Auth.Tests, Seguros.Tests) y pruebas unitarias para handlers críticos.

---

## Troubleshooting común
- JWT inválido:
  - Verifica que JwtSettings.SecretKey, Issuer y Audience sean idénticos en Auth y Seguros.
  - Comprueba el formato del header Authorization (Bearer <token>).
- Conexión a base de datos:
  - Asegúrate de que ConnectionStrings apunten al servidor correcto y que la DB existe.
  - Nota: AddSegurosServices usa la key "InsuranceDB" para obtener la cadena de conexión.
- Redis no funciona:
  - Verifica Redis:ConnectionString y que el servicio Redis esté accesible.
- Errores de logging:
  - Comprueba permisos de escritura en la carpeta `Logs`.
- Puertos en conflicto:
  - Ajusta launchSettings.json o variables de entorno para cambiar puertos HTTPS/HTTP.
- SecretKey inseguro:
  - No almacenes SecretKey en repositorio público; usa secretos de entorno en producción (Azure Key Vault, AWS Secrets Manager, variables de entorno).

---

## Operación y despliegue
- En desarrollo: usar ASPNETCORE_ENVIRONMENT=Development para habilitar Swagger en raíz y políticas permisivas.
- En producción:
  - configurar HTTPS y certificados apropiados,
  - aplicar políticas de CORS restrictivas,
  - proteger SecretKey (no checked-in),
  - persistir y rotar logs según la política de la organización.
- Despliegue con Docker: el proyecto incluye un docker-compose.yml en `src/` (ver Microservicios.Auth.sln SolutionItems). Puedes generar Dockerfiles por servicio y un docker-compose para orquestar Auth, Seguros y Redis/SQL.

---

## Contribuir
- Workflow recomendado:
  - Crear branch feature/xxx
  - Añadir tests unitarios para new behaviour
  - Abrir PR con descripción y evidencia (curl/Swagger)
- Contacto en el código: Autor Swashbuckle OpenAPI contact: Alexis Lael Lara Rodriguez (efestos.736@gmail.com)

---

## Archivos relevantes (puntos de entrada)
- Microservicios.Auth.sln
- src/Auth/Auth.WebAPI/Program.cs
- src/Auth/Auth.WebAPI/Extensions/ServiceCollectionExtensions.cs
- src/Auth/Auth.WebAPI/appsettings.json
- src/Auth/Auth.WebAPI/Controllers/AuthController.cs
- src/Auth/Auth.Infrastructure/Services/TokenService.cs
- src/Auth/Auth.Infrastructure/Middleware/CustomMiddleware.cs
- src/Seguros/Seguros.WebAPI/Program.cs
- src/Seguros/Seguros.WebAPI/Extensions/ServiceCollectionExtensions.cs
- src/Seguros/Seguros.WebAPI/Controllers/ClientesController.cs
- src/Seguros/Seguros.WebAPI/Controllers/PolizasController.cs

---

## Contacto
- Autor / Maintainer: ver OpenApi contact en ServiceCollectionExtensions: Alexis Lael Lara Rodriguez — efestos.736@gmail.com

---

---
