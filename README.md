# Microservicio de Seguros

## Arquitectura Clean Architecture + CQRS + MediatR

### Estructura de Capas

```
├── Seguros.Core (Contratos)
│   ├── Commands/          # Comandos CQRS
│   ├── Queries/           # Queries CQRS
│   ├── DTOs/              # Data Transfer Objects
│   ├── Enums/             # Enumeraciones
│   └── Interfaces/        # Contratos
│
├── Seguros.Domain (Lógica de Negocio)
│   └── Entities/          # Entidades del dominio
│
├── Seguros.Application (Casos de Uso)
│   └── Handlers/          # Command & Query Handlers
│
├── Seguros.Infrastructure (Servicios Externos)
│   ├── Data/              # Repositorios Dapper
│   └── Services/          # Servicios externos (AuthApiService, CacheService)
│
└── Seguros.WebAPI (Presentación)
    ├── Controllers/       # API Controllers
    └── Extensions/        # Configuración
```

---

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "InsuranceDB": "Server=sqlserver;Database=InsuranceDB;User Id=sa;Password=SQLsa736.;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "AuthSecretKey2026!@#$%^&*()_+",
    "Issuer": "Auth.API",
    "Audience": "Seguros.Microservicios",
    "ExpiryInHours": 1
  },
  "AuthApiUrl": "http://localhost:5001",
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "Seguros:"
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 5,
    "ClienteExpirationMinutes": 10,
    "PolizaExpirationMinutes": 15
  }
}
```

**Importante:** 
- Los JwtSettings deben ser idénticos a los del microservicio Auth para validar los tokens correctamente.
- En Docker, `Redis:ConnectionString` debe ser `redis:6379` (nombre del servicio).
- En desarrollo local, usar `localhost:6379`.

---

## Endpoints

### Seguros
- `GET /api/seguros` - Obtener todos los seguros (requiere autenticación)
- `GET /api/seguros/{id}` - Obtener seguro por ID (requiere autenticación)

### Health Check
- `GET /health` - Estado del servicio

---

## Diferencias con Auth

| Aspecto | Auth | Seguros |
|---------|------|---------|
| Puerto | 5001 | 5002 |
| Base de datos | AuthDB | SegurosDB |
| JWT | Genera tokens | Valida tokens |
| Propósito | Autenticación | Gestión de seguros |

---

## Ejecutar el Microservicio

```bash
cd src/Seguros/Seguros.WebAPI
dotnet run
```

Accede a: `https://localhost:5001`

---

---

## Implementación de Redis Cache

### Descripción

Se ha implementado Redis como sistema de caché distribuido para mejorar el rendimiento del microservicio, reduciendo la carga en la base de datos y mejorando los tiempos de respuesta de las consultas frecuentes.

### Arquitectura

```
┌─────────────────┐
│   Controllers    │
└────────┬─────────┘
         │
┌────────▼─────────┐
│  Query Handlers  │ ──┐
└──────────────────┘   │
                       │
         ┌─────────────┘
         │
┌────────▼─────────┐      ┌──────────────┐
│  ICacheService   │◄─────│  CacheService│
│    (Core)        │      │(Infrastructure)
└────────┬─────────┘      └──────┬───────┘
         │                        │
         │                        │
┌────────▼────────────────────────▼──────┐
│         Redis (Distributed Cache)      │
└────────────────────────────────────────┘
```

### Componentes

#### 1. Interfaz (Core Layer)
- **Ubicación:** `Seguros.Core.Interfaces.Application.ICacheService`
- **Responsabilidad:** Define el contrato para el servicio de caché
- **Métodos:**
  - `GetAsync<T>(string key)` - Obtener valor del caché
  - `SetAsync<T>(string key, T value, TimeSpan? expiration)` - Almacenar en caché
  - `RemoveAsync(string key)` - Eliminar clave específica
  - `RemoveByPatternAsync(string pattern)` - Eliminar por patrón (futuro)

#### 2. Implementación (Infrastructure Layer)
- **Ubicación:** `Seguros.Infrastructure.Services.CacheService`
- **Responsabilidad:** Implementa la lógica de caché usando `IDistributedCache`
- **Características:**
  - Serialización JSON automática
  - Manejo de errores con logging
  - Expiración configurable
  - Fallback a memoria si Redis no está disponible

#### 3. Integración en Handlers

**Query Handlers con Caché:**
- `GetClienteByUserIdQueryHandler` - Caché clave: `cliente:userid:{userId}`
- `GetAllClientesQueryHandler` - Caché clave: `clientes:all`

**Command Handlers con Invalidación:**
- `CreateClienteCommandHandler` - Invalida `clientes:all`
- `UpdateClienteCommandHandler` - Invalida todas las claves relacionadas
- `DeleteClienteCommandHandler` - Invalida todas las claves relacionadas

### Configuración

#### Docker Compose

Redis está configurado en `docker-compose.yml`:

```yaml
redis:
  image: redis:7-alpine
  container_name: redis
  ports:
    - "6379:6379"
  volumes:
    - redis_data:/data
  networks:
    - backend
  command: redis-server --appendonly yes
```

#### Variables de Entorno

En `docker-compose.yml` para `seguros-api`:
```yaml
environment:
  - Redis__ConnectionString=redis:6379
```

#### appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",  // En Docker: "redis:6379"
    "InstanceName": "Seguros:"
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 5,
    "ClienteExpirationMinutes": 10,
    "PolizaExpirationMinutes": 15
  }
}
```

### Uso en Handlers

#### Ejemplo: Query Handler con Caché

```csharp
public class GetClienteByUserIdQueryHandler : IRequestHandler<GetClienteByUserIdQuery, BaseResponse<ClienteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;

    public async Task<BaseResponse<ClienteDto>> Handle(GetClienteByUserIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Intentar obtener del caché
        var cacheKey = $"cliente:userid:{request.Id}";
        var cachedResult = await _cacheService.GetAsync<BaseResponse<ClienteDto>>(cacheKey, cancellationToken);
        
        if (cachedResult != null && cachedResult.Success)
        {
            return cachedResult; // Cache hit
        }

        // 2. Si no está en caché, obtener de la BD
        var cliente = await _unitOfWork.Clientes.GetByUserIdAsync(request.Id);
        
        // 3. Construir respuesta
        var result = new BaseResponse<ClienteDto> { /* ... */ };

        // 4. Almacenar en caché
        var expirationMinutes = _configuration.GetValue<int>("CacheSettings:ClienteExpirationMinutes", 10);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(expirationMinutes), cancellationToken);

        return result;
    }
}
```

#### Ejemplo: Command Handler con Invalidación

```csharp
public class UpdateClienteCommandHandler : IRequestHandler<UpdateClienteCommand, BaseResponse<ClienteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public async Task<BaseResponse<ClienteDto>> Handle(UpdateClienteCommand request, CancellationToken cancellationToken)
    {
        // ... lógica de actualización ...

        // Invalidar caché relacionado
        await _cacheService.RemoveAsync($"cliente:userid:{cliente.UserId}", cancellationToken);
        await _cacheService.RemoveAsync($"cliente:id:{cliente.Id}", cancellationToken);
        await _cacheService.RemoveAsync($"cliente:identificacion:{cliente.NumeroIdentificacion}", cancellationToken);
        await _cacheService.RemoveAsync("clientes:all", cancellationToken);

        return result;
    }
}
```

### Estrategia de Claves de Caché

Las claves siguen un patrón consistente:

- `cliente:userid:{userId}` - Cliente por UserId
- `cliente:id:{id}` - Cliente por ID
- `cliente:identificacion:{numero}` - Cliente por identificación
- `clientes:all` - Lista de todos los clientes
- `poliza:id:{id}` - Póliza por ID (futuro)
- `polizas:all` - Lista de todas las pólizas (futuro)

### Tiempos de Expiración

Los tiempos de expiración son configurables por tipo de dato:

- **DefaultExpirationMinutes:** 5 minutos (para datos generales)
- **ClienteExpirationMinutes:** 10 minutos (para datos de clientes)
- **PolizaExpirationMinutes:** 15 minutos (para datos de pólizas)

### Beneficios

1. **Performance:** Reducción significativa en tiempos de respuesta (de segundos a milisegundos)
2. **Escalabilidad:** Múltiples instancias comparten el mismo caché
3. **Reducción de Carga:** Menos consultas a la base de datos
4. **Resiliencia:** Fallback automático a memoria si Redis no está disponible

### Monitoreo

El servicio de caché registra automáticamente:
- **Cache Hits:** Cuando se encuentra un valor en caché
- **Cache Misses:** Cuando no se encuentra y se consulta la BD
- **Errores:** Problemas de conexión o serialización

Los logs se pueden ver en:
- Consola (Serilog)
- Archivo: `Logs/Seguros-{fecha}.txt`

### Troubleshooting

#### Redis no disponible
Si Redis no está disponible, el sistema automáticamente usa `IDistributedMemoryCache` como fallback. Verás un warning en los logs:
```

