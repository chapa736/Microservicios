# Microservicio de Seguros - Finasist

## Arquitectura Clean Architecture + CQRS + MediatR

### Estructura de Capas

```
├── Finasist.Seguros.Core (Contratos)
│   ├── Commands/          # Comandos CQRS
│   ├── Queries/           # Queries CQRS
│   ├── DTOs/              # Data Transfer Objects
│   ├── Enums/             # Enumeraciones
│   └── Interfaces/        # Contratos
│
├── Finasist.Seguros.Domain (Lógica de Negocio)
│   └── Entities/          # Entidades del dominio
│
├── Finasist.Seguros.Application (Casos de Uso)
│   └── Handlers/          # Command & Query Handlers
│
├── Finasist.Seguros.Infrastructure (Servicios Externos)
│   ├── Data/              # Repositorios Dapper
│   └── Services/          # Servicios externos
│
└── Finasist.Seguros.WebAPI (Presentación)
    ├── Controllers/       # API Controllers
    └── Extensions/        # Configuración
```

---

## Configuración

### appsettings.json

```json
{
  "ConnectionStrings": {
    "SegurosDB": "Server=.;Database=FinasistSeguros;..."
  },
  "JwtSettings": {
    "SecretKey": "same-as-auth",
    "Issuer": "Finasist.Auth.API",
    "Audience": "Finasist.Clients"
  }
}
```

**Importante:** Los JwtSettings deben ser idénticos a los del microservicio Auth para validar los tokens correctamente.

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
| Puerto | 7000/7001 | 5000/5001 |
| Base de datos | AuthDB | SegurosDB |
| JWT | Genera tokens | Valida tokens |
| Propósito | Autenticación | Gestión de seguros |

---

## Ejecutar el Microservicio

```bash
cd src/Seguros/Finasist.Seguros.WebAPI
dotnet run
```

Accede a: `https://localhost:5001`

---

## Próximos Pasos

1. Implementar Commands y Queries en Core
2. Crear Handlers en Application
3. Implementar repositorios Dapper en Infrastructure
4. Agregar validaciones con FluentValidation
5. Implementar Unit Tests
