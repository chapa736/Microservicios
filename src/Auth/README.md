# Microservicio de Autenticación - Finasist

## Arquitectura Clean Architecture + CQRS + MediatR

### Estructura de Capas

```
├── Finasist.Auth.Core (Contratos)
│   ├── Commands/          # Comandos CQRS (escritura)
│   ├── Queries/           # Queries CQRS (lectura)
│   ├── DTOs/              # Data Transfer Objects
│   ├── Enums/             # Enumeraciones
│   ├── Common/            # Clases base y constantes
│   └── Interfaces/
│       ├── Domain/        # IRepositories, IUnitOfWork
│       └── Application/   # ITokenService
│
├── Finasist.Auth.Domain (Lógica de Negocio)
│   └── Entities/          # Entidades del dominio
│
├── Finasist.Auth.Application (Casos de Uso)
│   └── Handlers/          # Command & Query Handlers (MediatR)
│       ├── AuthCommandHandlers.cs
│       └── AuthQueryHandlers.cs
│
├── Finasist.Auth.Infrastructure (Servicios Externos)
│   ├── Data/              # Repositorios Dapper
│   │   ├── DapperRepositories.cs
│   │   └── DapperUnitOfWork.cs
│   ├── Services/          # TokenService (JWT)
│   ├── Middleware/        # Exception & Request Logging
│   └── Extensions/        # ServiceCollection Extensions
│       ├── ServiceCollectionExtensions.cs
│       ├── SwaggerExtensions.cs
│       └── LoggingExtensions.cs
│
├── Finasist.Auth.Persistence (EF Core - Opcional)
│   ├── Context/           # DbContext
│   └── Repositories/      # Repositorios EF Core
│
└── Finasist.Auth.WebAPI (Presentación)
    ├── Controllers/       # API Controllers
    └── Program.cs         # Configuración del pipeline
```

---

## Flujo de Datos (CQRS)

### Commands (Escritura)
```
Controller → Command → MediatR → CommandHandler → UnitOfWork → Dapper → SQL Server
```

### Queries (Lectura)
```
Controller → Query → MediatR → QueryHandler → UnitOfWork → Dapper → SQL Server
```

---

## Responsabilidades por Capa

### 1. Core (Contratos)
- **Commands & Queries**: Define las operaciones (IRequest<TResponse>)
- **DTOs**: Objetos de transferencia sin lógica
- **Enums**: Estados y tipos
- **Interfaces**: Contratos para repositorios y servicios

### 2. Domain (Lógica de Negocio)
- **Entities**: Modelos de dominio con reglas de negocio
- **Value Objects**: Objetos inmutables
- **Domain Services**: Lógica de negocio compleja

### 3. Application (Casos de Uso)
- **Handlers**: Implementan Commands y Queries
- **Validators**: FluentValidation (opcional)
- **Mappers**: AutoMapper (opcional)

### 4. Infrastructure (Acceso a Datos)
- **Dapper Repositories**: Consultas SQL optimizadas
- **UnitOfWork**: Gestión de transacciones
- **TokenService**: Generación y validación JWT
- **Middleware**: Logging y manejo de excepciones
- **Extensions**: Configuración de servicios

### 5. Persistence (EF Core - Opcional)
- **DbContext**: Configuración de EF Core
- **Repositories**: Implementación con LINQ

### 6. WebAPI (Presentación)
- **Controllers**: Endpoints REST
- **Program.cs**: Pipeline de middleware

---

## Tecnologías Utilizadas

- **.NET 6+**
- **MediatR**: Patrón CQRS
- **Dapper**: Micro ORM para consultas optimizadas
- **JWT**: Autenticación basada en tokens
- **Serilog**: Logging estructurado
- **Swagger**: Documentación API
- **BCrypt.Net**: Hash de contraseñas
- **SQL Server**: Base de datos

---

## Configuración (appsettings.json)

```json
{
  "ConnectionStrings": {
    "AuthDB": "Server=.;Database=FinasistAuth;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "Finasist.Auth.API",
    "Audience": "Finasist.Clients"
  },
  "TamañoMaximoLogEnMegas": 10
}
```

---

## Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/refresh` - Refrescar token
- `POST /api/auth/logout` - Cerrar sesión

### Health Check
- `GET /health` - Estado del servicio

---

## Ventajas de esta Arquitectura

1. **Separación de Responsabilidades**: Cada capa tiene un propósito claro
2. **Testeable**: Fácil de hacer unit tests por capa
3. **Mantenible**: Cambios en una capa no afectan otras
4. **Escalable**: Fácil agregar nuevas funcionalidades
5. **Performance**: Dapper para consultas optimizadas
6. **CQRS**: Separación de lectura/escritura
7. **Clean Code**: Código limpio y organizado
