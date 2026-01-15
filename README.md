# Microservicios

Microservicios para el sistema de gestión de pólizas. Incluye:
- Auth: microservicio de autenticación (genera JWT).
- Seguros: microservicio para gestión de clientes y pólizas (valida JWT).

Este repositorio sigue una arquitectura basada en Clean Architecture + CQRS (MediatR) con Dapper para acceso a datos, Serilog para logging y Swagger para documentación API.

---

## Contenido
- src/Auth: microservicio de Autenticación
  - Controllers, Handlers, TokenService, Middleware, configuración de Serilog y JWT.
  - Documentación interna: GUIA_DESARROLLO.md (guía para añadir funcionalidades).
- src/Seguros: microservicio de Seguros
  - Endpoints para Clientes y Pólizas, validación JWT y autorización por roles.
- Microservicios.Auth.sln: solución con ambos microservicios.

---

## Arquitectura y tecnologías principales
- Clean Architecture: Core / Domain / Application / Infrastructure / WebAPI
- CQRS + MediatR (Handlers para Commands y Queries)
- Dapper (acceso a datos)
- JWT (System.IdentityModel.Tokens.Jwt) — Token generation en Auth, token validation en Seguros
- Serilog (console + file rolling)
- Swagger (Swashbuckle) para documentación de API
- BCrypt.Net para hashing de contraseñas
- SQL Server (se usa en ejemplos de configuración)
- .NET 6

---

## Requisitos previos
- .NET 6 SDK (dotnet)
- SQL Server (o una instancia compatible)
- (Opcional) Visual Studio 2022
- Acceso para escribir en carpeta `Logs` (permiso para generar ficheros de log)

---

## Quick start (local)

Desde la raíz del repositorio:

1. Restaurar y compilar la solución:
   - dotnet build Microservicios.Auth.sln

2. Ejecutar Auth API:
   - dotnet run --project src/Auth/Auth.WebAPI
   - Por defecto Swagger (si el entorno es Development) estará en la raíz del host (ej. https://localhost:5001 o en el puerto configurado por Kestrel). Ver Program.cs para el pipeline.

3. Ejecutar Seguros API:
   - dotnet run --project src/Seguros/Seguros.WebAPI
   - Swagger en https://localhost:5001 (o puerto que Kestrel asigne).

Nota: Ejecuta cada servicio en terminal separada. Ajusta puertos si necesitas correr ambos en HTTPS/HTTP al mismo tiempo (usar launchSettings.json o variables de entorno).

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

Variables / valores críticos:
- ConnectionStrings: `AuthDB` (Auth), `InsuranceDB` / `SegurosDB` (Seguros). Asegúrate de apuntar al servidor SQL correcto.
- JwtSettings.SecretKey: debe tener longitud suficiente y mantenerse seguro (no en repositorio).
- TamanhoMaximoLogEnMegas: control para Serilog fileSizeLimitBytes.

Puedes usar variables de entorno:
- ASPNETCORE_ENVIRONMENT (Development/Production)
- ConnectionStrings__AuthDB
- JwtSettings__SecretKey
- JwtSettings__Issuer
- JwtSettings__Audience

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

curl:
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

Seguros define políticas y roles (ej. ADMINISTRADOR, CLIENTE). Asegúrate de que el JWT incluya roles (TokenService añade ClaimTypes.Role por cada rol en UserDto.Roles).

---

## Endpoints principales - Seguros (resumen)

Controladores y rutas principales (ver src/Seguros/Seguros.WebAPI/Controllers):

ClientesController:
- GET /api/clientes (Admin)
- GET /api/clientes/{id} (Admin)
- GET /api/clientes/identificacion/{numeroIdentificacion} (Admin)
- POST /api/clientes (Admin)
- PUT /api/clientes/{id} (Admin)
- PATCH /api/clientes/mi-informacion (Cliente)
- DELETE /api/clientes/{id} (Admin)

PolizasController:
- GET /api/polizas/{id}
- GET /api/polizas/cliente/{clienteId}
- GET /api/polizas/tipo/{tipo} (Admin)
- GET /api/polizas/estatus/{estatus} (Admin)
- GET /api/polizas/vigentes (Admin)
- POST /api/polizas (Admin)
- POST /api/polizas/{id}/cancelar (Cliente)

---

## Logging & Health checks
- Serilog configurado para consola y fichero: Logs/Auth-YYYYMMDD.txt / Logs/Seguros-YYYYMMDD.txt
- Health checks expuestos en `/health` (MapHealthChecks).
- Configuración de Serilog y tamaño de fichero controlado por `TamanhoMaximoLogEnMegas`.

---

## Middleware y cross-cutting concerns
- ExceptionHandlingMiddleware: centraliza manejo de excepciones y devuelve BaseResponse con errores.
- RequestLoggingMiddleware: logs de inicio/fin de request.
- JwtBearer Events: OnAuthenticationFailed, OnTokenValidated, OnChallenge y OnMessageReceived están loggeados para diagnóstico.

---

## Desarrollo: cómo agregar nuevas funcionalidades
Consulta `src/Auth/GUIA_DESARROLLO.md` para ejemplos y checklist. Resumen de pasos al agregar Command/Query:

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
- Actualmente la solución no incluye proyectos de test en la raíz; añade proyectos de test por servicio si es necesario.

---

## Troubleshooting común
- JWT inválido:
  - Verifica que JwtSettings.SecretKey, Issuer y Audience sean idénticos en Auth y Seguros.
  - Comprueba el formato del header Authorization (Bearer <token>).
- Conexión a base de datos:
  - Asegúrate de que ConnectionStrings apunten al servidor correcto y que la DB existe.
- Errores de logging:
  - Comprueba permisos de escritura en la carpeta `Logs`.
- Puertos en conflicto:
  - Ajusta launchSettings.json o variables de entorno para cambiar puertos HTTPS/HTTP.
- SecretKey inseguro:
  - No almacenes SecretKey en repositorio público; usa secretos de entorno en producción (Azure Key Vault, AWS Secrets Manager, variables de entorno).

---

## Operación y despliegue
- En desarrollo: usar ASPNETCORE_ENVIRONMENT=Development para habilitar Swagger en raíz.
- En producción: configurar HTTPS, agregar políticas de CORS de producción y ajustar `TamanhoMaximoLogEnMegas`.
- Persistir y rotar logs según la política de la organización.
- Asegurar la protección de SecretKey (no checked-in).

---

## Contribuir
- Lee `src/Auth/GUIA_DESARROLLO.md` para convenciones y checklist.
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
- src/Auth/GUIA_DESARROLLO.md (guía de desarrollo)

---

## Contacto
- Autor / Maintainer: ver OpenApi contact en ServiceCollectionExtensions: Alexis Lael Lara Rodriguez — efestos.736@gmail.com

---
