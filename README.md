# Documento Técnico - Sistema de Microservicios Auth y Seguros

**Versión:** 1.0  
**Fecha:** Enero 2026  
**Autor:** Proyecto Microservicios  
**Plataforma:** .NET 8.0

---

## Tabla de Contenidos

1. [Descripción General](#1-descripción-general)
2. [Arquitectura del Sistema](#2-arquitectura-del-sistema)
3. [Tecnologías Utilizadas](#3-tecnologías-utilizadas)
4. [Estructura de Proyectos](#4-estructura-de-proyectos)
5. [Bases de Datos](#5-bases-de-datos)
6. [APIs y Endpoints](#6-apis-y-endpoints)
7. [DTOs y Modelos](#7-dtos-y-modelos)
8. [Commands y Queries (CQRS)](#8-commands-y-queries-cqrs)
9. [Seguridad y Autenticación](#9-seguridad-y-autenticación)
10. [Cache y Performance](#10-cache-y-performance)
11. [Logging](#11-logging)
12. [Configuración](#12-configuración)
13. [Infraestructura y DevOps](#13-infraestructura-y-devops)
14. [Diagramas de Arquitectura](#14-diagramas-de-arquitectura)
15. [Flujos de Datos](#15-flujos-de-datos)

---

## 1. Descripción General

### 1.1. Visión General

Este proyecto implementa un sistema de microservicios para la gestión de autenticación/autorización y seguros, desarrollado con **.NET 8.0** siguiendo los principios de **Clean Architecture** y el patrón **CQRS** (Command Query Responsibility Segregation) utilizando **MediatR**.

### 1.2. Microservicios

El sistema está compuesto por dos microservicios principales:

#### 1.2.1. Auth API (Microservicio de Autenticación)
- **Propósito**: Gestión de usuarios, autenticación y autorización
- **Responsabilidades**:
  - Registro y autenticación de usuarios
  - Generación y validación de tokens JWT
  - Gestión de roles y permisos
  - Refresh tokens para renovación de sesiones
- **URL Producción**: `https://auth-api-hscra6bsgahsepdb.westus2-01.azurewebsites.net`

#### 1.2.2. Seguros API (Microservicio de Gestión de Seguros)
- **Propósito**: Gestión de clientes y pólizas de seguros
- **Responsabilidades**:
  - CRUD de clientes
  - CRUD de pólizas de seguros
  - Validación de tokens JWT generados por Auth API
  - Cache distribuido con Redis
- **URL Producción**: `https://seguros-api-allr-hycycshxhsgah8eq.westus2-01.azurewebsites.net`

### 1.3. Principios Arquitectónicos

- **Clean Architecture**: Separación en capas con dependencias que apuntan hacia el dominio
- **CQRS**: Separación de comandos (escritura) y queries (lectura)
- **Dependency Injection**: Inversión de dependencias para desacoplamiento
- **Repository Pattern**: Abstracción del acceso a datos
- **Unit of Work**: Gestión transaccional de operaciones
- **Microservicios**: Servicios independientes y desacoplados

---

## 2. Arquitectura del Sistema

### 2.1. Patrones Implementados

#### 2.1.1. Clean Architecture

Cada microservicio sigue la estructura de Clean Architecture con las siguientes capas:

```
┌─────────────────────────────────────────────────────────┐
│                   WebAPI (Presentación)                  │
│  - Controllers                                           │
│  - Middleware                                           │
│  - Extensions (Configuraciones)                         │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│              Application (Casos de Uso)                  │
│  - Handlers (Command & Query Handlers)                  │
│  - DTOs Application                                      │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│                  Domain (Lógica de Negocio)              │
│  - Entities                                             │
│  - Domain Logic                                         │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│              Infrastructure (Acceso a Datos)             │
│  - Repositories (Dapper)                                │
│  - Services (TokenService, CacheService, AuthApiService)│
│  - Middleware                                           │
└──────────────────┬──────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────┐
│                  Core (Contratos)                        │
│  - Commands & Queries (CQRS)                            │
│  - DTOs                                                 │
│  - Interfaces                                           │
│  - Enums                                                │
└──────────────────────────────────────────────────────────┘
```

**Reglas de Dependencias:**
- Las capas externas dependen de las internas
- El Domain NO depende de nada externo
- Core define los contratos sin implementación

#### 2.1.2. CQRS (Command Query Responsibility Segregation)

**Separación de Responsabilidades:**

- **Commands**: Operaciones de escritura (Create, Update, Delete)
  - Ubicación: `{Microservicio}.Core.Commands`
  - Implementación: `{Microservicio}.Application.Handlers.CommandHandlers`
  - Ejemplos: `CreateClienteCommand`, `LoginCommand`, `CreatePolizaCommand`

- **Queries**: Operaciones de lectura (Get, GetAll, Search)
  - Ubicación: `{Microservicio}.Core.Queries`
  - Implementación: `{Microservicio}.Application.Handlers.QueryHandlers`
  - Ejemplos: `GetClienteByIdQuery`, `GetAllClientesQuery`, `GetUserByUsernameQuery`

#### 2.1.3. MediatR

**Propósito**: Implementa el patrón Mediator para CQRS

- **Beneficios**:
  - Desacoplamiento entre Controllers y Handlers
  - Pipeline de comportamiento (validation, logging, etc.)
  - Fácil testing
- **Versión**: 12.2.0

### 2.2. Estructura de Capas Detallada

#### 2.2.1. Capa Core (Contratos)

**Responsabilidades:**
- Define interfaces y contratos
- Commands y Queries para CQRS
- DTOs para transferencia de datos
- Enumeraciones y constantes
- Clases base para respuestas

**Estructura:**
```
Core/
├── Commands/          # Comandos CQRS (IRequest)
├── Queries/           # Queries CQRS (IRequest)
├── DTOs/              # Data Transfer Objects
├── Enums/             # Enumeraciones
├── Interfaces/        # Contratos (IRepositories, IServices)
│   ├── Application/   # Interfaces de servicios de aplicación
│   └── Domain/        # Interfaces de repositorios y UoW
└── Common/            # Clases base (BaseResponse, Constants)
```

#### 2.2.2. Capa Domain (Lógica de Negocio)

**Responsabilidades:**
- Entidades del dominio con reglas de negocio
- Lógica de dominio pura (sin dependencias externas)
- Validaciones de negocio

**Estructura:**
```
Domain/
└── Entities/          # Entidades del dominio
```

**Entidades Principales:**

**Auth.Domain:**
- `User`: Usuario del sistema
- `Role`: Rol de usuario
- `UserRole`: Relación muchos-a-muchos entre User y Role
- `RefreshToken`: Tokens de refresco

**Seguros.Domain:**
- `Cliente`: Cliente del sistema de seguros
- `Poliza`: Póliza de seguro

#### 2.2.3. Capa Application (Casos de Uso)

**Responsabilidades:**
- Implementa la lógica de casos de uso
- Handlers para Commands y Queries
- Coordinación entre repositorios y servicios
- Validaciones de aplicación

**Estructura:**
```
Application/
└── Handlers/          # Command & Query Handlers
    ├── CommandHandlers.cs
    └── QueryHandlers.cs
```

#### 2.2.4. Capa Infrastructure (Acceso a Datos)

**Responsabilidades:**
- Implementación de repositorios con Dapper
- Servicios externos (TokenService, CacheService)
- Configuración de dependencias
- Middleware personalizado
- Acceso a bases de datos y servicios externos

**Estructura:**
```
Infrastructure/
├── Data/              # Repositorios Dapper
│   └── DapperRepositories.cs
├── Services/          # Servicios externos
│   ├── TokenService.cs (Auth)
│   ├── CacheService.cs (Seguros)
│   └── AuthApiService.cs (Seguros)
└── Middleware/        # Middleware personalizado
    ├── ExceptionHandlingMiddleware.cs
    └── RequestLoggingMiddleware.cs (Auth)
```

#### 2.2.5. Capa WebAPI (Presentación)

**Responsabilidades:**
- Controllers REST API
- Configuración del pipeline de middleware
- Swagger/OpenAPI documentation
- Configuración de servicios

**Estructura:**
```
WebAPI/
├── Controllers/       # API Controllers
│   ├── AuthController.cs
│   ├── ClientesController.cs
│   └── PolizasController.cs
├── Extensions/        # Extensiones y configuraciones
│   └── ServiceCollectionExtensions.cs
├── Program.cs         # Bootstrap de la aplicación
└── appsettings.json   # Configuración
```

---

## 3. Tecnologías Utilizadas

### 3.1. Framework y Plataforma

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 8.0 | Framework de desarrollo principal |
| **ASP.NET Core** | 8.0 | Framework web para APIs REST |
| **C#** | 12.0 | Lenguaje de programación |
| **SQL Server** | 2022 | Sistema de gestión de bases de datos relacional |

### 3.2. Paquetes NuGet - Microservicio Auth

#### 3.2.1. Proyecto: Auth.WebAPI

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Autenticación JWT Bearer |
| `MediatR` | 12.2.0 | Patrón Mediator para CQRS |
| `Swashbuckle.AspNetCore` | 6.5.0 | Documentación Swagger/OpenAPI |
| `System.IdentityModel.Tokens.Jwt` | 7.1.2 | Manipulación de tokens JWT |

#### 3.2.2. Proyecto: Auth.Application

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `BCrypt.Net-Next` | 4.0.3 | Hash seguro de contraseñas |
| `MediatR` | 12.2.0 | Integración con Commands/Queries |
| `FluentValidation` | 11.8.0 | Validación fluida de modelos |
| `AutoMapper` | 12.0.1 | Mapeo automático de objetos |
| `Serilog` | 3.1.1 | Framework de logging estructurado |
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones con atributos |

#### 3.2.3. Proyecto: Auth.Infrastructure

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Dapper` | 2.1.24 | Micro ORM para acceso a datos |
| `Microsoft.Data.SqlClient` | 5.2.2 | Cliente SQL Server para .NET |
| `AspNetCore.HealthChecks.SqlServer` | 9.0.0 | Health checks para SQL Server |
| `Serilog.AspNetCore` | 8.0.0 | Integración de Serilog con ASP.NET Core |
| `Serilog.Sinks.File` | 5.0.0 | Escritura de logs en archivos |
| `Swashbuckle.AspNetCore` | 6.5.0 | Documentación Swagger |
| `System.IdentityModel.Tokens.Jwt` | 7.1.2 | Tokens JWT |
| `Microsoft.IdentityModel.Tokens` | 7.1.2 | Validación de tokens |

#### 3.2.4. Proyecto: Auth.Core

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `MediatR` | 12.2.0 | Contratos de Commands/Queries |
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones |

#### 3.2.5. Proyecto: Auth.Domain

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones en entidades |

### 3.3. Paquetes NuGet - Microservicio Seguros

#### 3.3.1. Proyecto: Seguros.WebAPI

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `MediatR` | 12.2.0 | Patrón Mediator para CQRS |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.0 | Validación de tokens JWT |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 8.0.0 | Cache distribuido con Redis |
| `Serilog` | 3.1.1 | Framework de logging |
| `Serilog.AspNetCore` | 8.0.0 | Integración con ASP.NET Core |
| `Serilog.Sinks.Console` | 5.0.1 | Logging en consola |
| `Serilog.Sinks.File` | 5.0.0 | Logging en archivos |
| `Swashbuckle.AspNetCore` | 6.5.0 | Documentación Swagger/OpenAPI |

#### 3.3.2. Proyecto: Seguros.Application

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Microsoft.Extensions.Configuration.Abstractions` | 8.0.0 | Configuración |
| `Serilog` | 3.1.1 | Logging |
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones |

#### 3.3.3. Proyecto: Seguros.Infrastructure

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `Dapper` | 2.1.24 | Micro ORM para acceso a datos |
| `Microsoft.Data.SqlClient` | 5.2.2 | Cliente SQL Server |
| `AspNetCore.HealthChecks.SqlServer` | 9.0.0 | Health checks |
| `Serilog.AspNetCore` | 8.0.0 | Logging |
| `Serilog.Sinks.File` | 5.0.0 | Logging en archivos |
| `Swashbuckle.AspNetCore` | 6.5.0 | Swagger |
| `System.IdentityModel.Tokens.Jwt` | 7.1.2 | Validación JWT |
| `Microsoft.IdentityModel.Tokens` | 7.1.2 | Validación de tokens |

#### 3.3.4. Proyecto: Seguros.Core

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `MediatR` | 12.2.0 | CQRS |
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones |

#### 3.3.5. Proyecto: Seguros.Domain

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| `System.ComponentModel.Annotations` | 5.0.0 | Validaciones |

### 3.4. Infraestructura y Herramientas

| Tecnología | Versión/Descripción | Uso |
|------------|---------------------|-----|
| **SQL Server** | 2022 Latest | Base de datos relacional |
| **Redis** | 7-alpine | Cache distribuido (solo Seguros) |
| **Docker** | Latest | Containerización |
| **Docker Compose** | 3.9 | Orquestación de contenedores |
| **Azure App Service** | - | Hosting en la nube |
| **Azure SQL Database** | - | Base de datos en la nube |
| **Azure Redis Cache** | - | Cache en la nube |
| **GitHub Actions** | - | CI/CD Pipeline |
| **Serilog** | 3.1.1 / 8.0.0 | Logging estructurado |
| **Swagger/OpenAPI** | 6.5.0 | Documentación de APIs |

---

## 4. Estructura de Proyectos

### 4.1. Estructura General

```
Microservicios/
├── .gitignore
├── Microservicios.Auth.sln
├── README.md
└── src/
    ├── Auth/
    │   ├── Auth.Application/
    │   ├── Auth.Core/
    │   ├── Auth.Domain/
    │   ├── Auth.Infrastructure/
    │   └── Auth.WebAPI/
    ├── Seguros/
    │   ├── Seguros.Application/
    │   ├── Seguros.Core/
    │   ├── Seguros.Domain/
    │   ├── Seguros.Infrastructure/
    │   └── Seguros.WebAPI/
    ├── docker/
    │   └── sql/
    │       ├── 01_SCRIPT CREATE BASES.sql
    │       ├── 02_SCRIPT CREATE TABLAS AUTH.sql
    │       └── 03_SCRIPT CREATE TABLAS INSURANCE.sql
    └── docker-compose.yml
```

### 4.2. Microservicio Auth - Estructura Detallada

```
Auth/
├── Auth.Application/
│   ├── DTOs/
│   │   └── AuthDTOs.cs
│   └── Handlers/
│       ├── AuthCommandHandlers.cs
│       └── AuthQueryHandlers.cs
│
├── Auth.Core/
│   ├── Commands/
│   │   └── AuthCommands.cs
│   ├── Queries/
│   │   └── AuthQueries.cs
│   ├── DTOs/
│   │   └── AuthDTOs.cs
│   ├── Enums/
│   │   └── AuthEnums.cs
│   ├── Interfaces/
│   │   ├── Application/
│   │   │   └── IServices.cs
│   │   └── Domain/
│   │       └── IRepositories.cs
│   └── Common/
│       └── BaseClasses.cs
│
├── Auth.Domain/
│   └── Entities/
│       └── AuthEntities.cs
│
├── Auth.Infrastructure/
│   ├── Data/
│   │   └── DapperRepositories.cs
│   ├── Services/
│   │   └── TokenService.cs
│   └── Middleware/
│       └── CustomMiddleware.cs
│
└── Auth.WebAPI/
    ├── Controllers/
    │   └── AuthController.cs
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs
    ├── Program.cs
    ├── appsettings.json
    ├── Dockerfile
    └── Properties/
        └── launchSettings.json
```

### 4.3. Microservicio Seguros - Estructura Detallada

```
Seguros/
├── Seguros.Application/
│   ├── DTOs/
│   │   └── SegurosDTOs.cs
│   └── Handlers/
│       ├── ClienteCommandHandlers.cs
│       ├── ClienteQueryHandlers.cs
│       ├── PolizaCommandHandlers.cs
│       └── PolizaQueryHandlers.cs
│
├── Seguros.Core/
│   ├── Commands/
│   │   ├── ClienteCommands.cs
│   │   └── PolizaCommands.cs
│   ├── Queries/
│   │   ├── ClienteQueries.cs
│   │   └── PolizaQueries.cs
│   ├── DTOs/
│   │   └── SegurosDTOs.cs
│   ├── Enums/
│   │   └── SegurosEnums.cs
│   ├── Exceptions/
│   │   └── CustomExceptions.cs
│   ├── Interfaces/
│   │   ├── Application/
│   │   │   ├── IAuthApiService.cs
│   │   │   └── ICacheService.cs
│   │   └── Domain/
│   │       └── IRepositories.cs
│   └── Common/
│       └── BaseClasses.cs
│
├── Seguros.Domain/
│   └── Entities/
│       └── SegurosEntities.cs
│
├── Seguros.Infrastructure/
│   ├── Data/
│   │   └── DapperRepositories.cs
│   ├── Services/
│   │   ├── AuthApiService.cs
│   │   └── CacheService.cs
│   └── Middleware/
│       └── ExceptionHandlingMiddleware.cs
│
└── Seguros.WebAPI/
    ├── Controllers/
    │   ├── ClientesController.cs
    │   └── PolizasController.cs
    ├── Extensions/
    │   └── ServiceCollectionExtensions.cs
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Dockerfile
    └── Properties/
        └── launchSettings.json
```

---

## 5. Bases de Datos

### 5.1. Descripción General

El sistema utiliza **dos bases de datos SQL Server** independientes:

1. **AuthDB**: Base de datos para autenticación y autorización
2. **InsuranceDB**: Base de datos para gestión de clientes y pólizas

### 5.2. Base de Datos: AuthDB

#### 5.2.1. Información General

- **Nombre**: `AuthDB`
- **Propósito**: Gestión de usuarios, roles y tokens de autenticación
- **Script de Creación**: `src/docker/sql/01_SCRIPT CREATE BASES.sql` y `02_SCRIPT CREATE TABLAS AUTH.sql`

#### 5.2.2. Tabla: Roles

**Descripción**: Almacena los roles del sistema (ADMINISTRADOR, CLIENTE)

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **Id** | INT IDENTITY | NO | Identificador único | PRIMARY KEY |
| **Nombre** | VARCHAR(50) | NO | Nombre del rol | UNIQUE, NOT NULL |
| **Descripcion** | VARCHAR(150) | YES | Descripción del rol | - |
| **Activo** | BIT | NO | Estado activo/inactivo | DEFAULT 1 |
| **FechaCreacion** | DATETIME2 | NO | Fecha de creación | DEFAULT SYSDATETIME() |

**Índices:**
- `UQ_Roles_Nombre` (UNIQUE CLUSTERED) - Garantiza nombres únicos

**Datos Iniciales:**
```sql
INSERT INTO Roles(Nombre, Descripcion, Activo)
VALUES 
    ('ADMINISTRADOR', 'ADMINISTRADOR GENERAL', 1),
    ('CLIENTE', 'CLIENTE', 1);
```

**Relaciones:**
- `Roles (1)` ──< `(N) UserRoles`

#### 5.2.3. Tabla: Users

**Descripción**: Almacena información de usuarios del sistema

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **Id** | INT IDENTITY | NO | Identificador único | PRIMARY KEY |
| **Username** | VARCHAR(50) | NO | Nombre de usuario | UNIQUE, NOT NULL |
| **Email** | VARCHAR(100) | NO | Correo electrónico | UNIQUE, NOT NULL |
| **PasswordHash** | VARCHAR(255) | NO | Hash de contraseña (BCrypt) | NOT NULL |
| **Activo** | BIT | NO | Estado activo/inactivo | DEFAULT 1 |
| **FechaCreacion** | DATETIME2 | NO | Fecha de creación | DEFAULT SYSDATETIME() |

**Índices:**
- `UQ_Users_Username` (UNIQUE NONCLUSTERED) - Garantiza usernames únicos
- `UQ_Users_Email` (UNIQUE NONCLUSTERED) - Garantiza emails únicos

**Relaciones:**
- `Users (1)` ──< `(N) UserRoles`
- `Users (1)` ──< `(N) RefreshTokens`

**Características:**
- Las contraseñas se almacenan como hash BCrypt
- No se almacenan contraseñas en texto plano
- El email y username deben ser únicos

#### 5.2.4. Tabla: UserRoles

**Descripción**: Tabla de relación muchos-a-muchos entre Users y Roles

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **UserId** | INT | NO | ID del usuario | PRIMARY KEY, FK → Users.Id |
| **RoleId** | INT | NO | ID del rol | PRIMARY KEY, FK → Roles.Id |
| **FechaCreacion** | DATETIME2 | NO | Fecha de asignación | DEFAULT SYSDATETIME() |

**Relaciones:**
- `FK_UserRoles_User` → `Users(Id)` (CASCADE DELETE)
- `FK_UserRoles_Role` → `Roles(Id)` (CASCADE DELETE)

**Características:**
- Un usuario puede tener múltiples roles
- Un rol puede ser asignado a múltiples usuarios
- Clave primaria compuesta (UserId, RoleId)

#### 5.2.5. Tabla: RefreshTokens

**Descripción**: Almacena tokens de refresco para renovar tokens de acceso

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **Id** | INT IDENTITY | NO | Identificador único | PRIMARY KEY |
| **UserId** | INT | NO | ID del usuario | FK → Users.Id |
| **Token** | VARCHAR(500) | NO | Token de refresco | NOT NULL |
| **FechaExp** | DATETIME2 | NO | Fecha de expiración | NOT NULL |
| **FechaCreacion** | DATETIME2 | NO | Fecha de creación | DEFAULT SYSDATETIME() |
| **Revocado** | BIT | NO | Token revocado | DEFAULT 0 |

**Relaciones:**
- `FK_RefreshTokens_User` → `Users(Id)`

**Índices:**
- `IX_RefreshTokens_UserId` (NONCLUSTERED) - Búsqueda por usuario

**Características:**
- Los tokens pueden ser revocados antes de expirar
- Se utilizan para obtener nuevos access tokens sin re-autenticación

#### 5.2.6. Diagrama de Relaciones - AuthDB

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│    Users    │         │  UserRoles   │         │    Roles    │
├─────────────┤         ├──────────────┤         ├─────────────┤
│ Id (PK)     │◄──┐     │ UserId (PK)  │     ┌──►│ Id (PK)     │
│ Username    │   │     │ RoleId (PK)  │◄────┤   │ Nombre      │
│ Email       │   │     │ FechaCreacion│     │   │ Descripcion │
│ PasswordHash│   │     └──────────────┘     │   │ Activo      │
│ Activo      │   │                          │   │ FechaCreacion│
│ FechaCreacion│  │                          │   └─────────────┘
└─────────────┘   │                          │
      │           │                          │
      │           └──────────────────────────┘
      │
      │           ┌──────────────┐
      └──────────►│ RefreshTokens│
                  ├──────────────┤
                  │ Id (PK)      │
                  │ UserId (FK)  │
                  │ Token        │
                  │ FechaExp     │
                  │ FechaCreacion│
                  │ Revocado     │
                  └──────────────┘
```

### 5.3. Base de Datos: InsuranceDB

#### 5.3.1. Información General

- **Nombre**: `InsuranceDB`
- **Propósito**: Gestión de clientes y pólizas de seguros
- **Script de Creación**: `src/docker/sql/01_SCRIPT CREATE BASES.sql` y `03_SCRIPT CREATE TABLAS INSURANCE.sql`

#### 5.3.2. Tabla: Clientes

**Descripción**: Almacena información de clientes del sistema de seguros

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **Id** | INT IDENTITY | NO | Identificador único | PRIMARY KEY |
| **NumeroIdentificacion** | VARCHAR(10) | NO | Número de identificación | UNIQUE, CHECK (10 dígitos numéricos) |
| **Nombre** | VARCHAR(40) | NO | Nombre del cliente | NOT NULL |
| **ApPaterno** | VARCHAR(40) | NO | Apellido paterno | NOT NULL |
| **ApMaterno** | VARCHAR(40) | NO | Apellido materno | NOT NULL |
| **Telefono** | VARCHAR(20) | NO | Teléfono de contacto | NOT NULL |
| **Email** | VARCHAR(100) | NO | Correo electrónico | NOT NULL |
| **Direccion** | VARCHAR(250) | YES | Dirección física | NULL |
| **FechaCreacion** | DATETIME2 | NO | Fecha de creación | DEFAULT SYSDATETIME() |
| **FechaActualizacion** | DATETIME2 | YES | Fecha de última actualización | NULL |
| **UserId** | INT | YES | ID del usuario (relación con Auth) | NULL |

**Restricciones:**
- `UQ_Clients_Identificacion` (UNIQUE) - Garantiza identificación única
- `CK_Clients_Identificacion` (CHECK) - Valida que sea exactamente 10 dígitos numéricos:
  ```sql
  CHECK (LEN(NumeroIdentificacion) = 10 AND NumeroIdentificacion NOT LIKE '%[^0-9]%')
  ```

**Índices:**
- `IX_Clientes_Apellidos_Nombre` (NONCLUSTERED) - Búsqueda por apellidos y nombre:
  ```sql
  CREATE NONCLUSTERED INDEX IX_Clientes_Apellidos_Nombre 
  ON dbo.Clientes(ApPaterno, ApMaterno, Nombre);
  ```
- `IX_Clientes_Email` (NONCLUSTERED) - Búsqueda por email:
  ```sql
  CREATE NONCLUSTERED INDEX IX_Clientes_Email 
  ON dbo.Clientes(Email);
  ```

**Relaciones:**
- `Clientes (1)` ──< `(N) Polizas`
- `UserId` referencia a `AuthDB.Users.Id` (relación lógica, no FK física)

**Características:**
- El `NumeroIdentificacion` debe ser exactamente 10 dígitos numéricos
- El `UserId` relaciona el cliente con un usuario del sistema de autenticación
- `FechaActualizacion` se actualiza automáticamente en operaciones UPDATE

#### 5.3.3. Tabla: Polizas

**Descripción**: Almacena información de pólizas de seguros

| Campo | Tipo | Nullable | Descripción | Restricciones |
|-------|------|----------|-------------|---------------|
| **Id** | INT IDENTITY | NO | Identificador único | PRIMARY KEY |
| **IdCliente** | INT | NO | ID del cliente | FK → Clientes.Id |
| **TipoPoliza** | INT | NO | Tipo de póliza (1-4) | NOT NULL |
| **FechaInicio** | DATE | NO | Fecha de inicio de vigencia | NOT NULL |
| **FechaFin** | DATE | NO | Fecha de fin de vigencia | NOT NULL |
| **Monto** | DECIMAL(18,2) | NO | Monto asegurado | NOT NULL |
| **Estatus** | INT | NO | Estatus (1-4) | NOT NULL |
| **FechaCreacion** | DATETIME2 | NO | Fecha de creación | DEFAULT SYSDATETIME() |

**Tipos de Póliza (TipoPoliza):**
- `1` = Vida
- `2` = Auto
- `3` = Hogar
- `4` = Salud

**Estatus de Póliza (Estatus):**
- `1` = Vigente
- `2` = Vencida
- `3` = Cancelada
- `4` = Suspendida

**Relaciones:**
- `FK_Polizas_Cliente` → `Clientes(Id)` (CASCADE DELETE)
  ```sql
  CONSTRAINT FK_Polizas_Cliente 
  FOREIGN KEY (IdCliente) REFERENCES Clientes(Id)
  ```

**Índices:**
- `IX_Polizas_IdCliente` (NONCLUSTERED) - Búsqueda por cliente:
  ```sql
  CREATE INDEX IX_Polizas_IdCliente ON Polizas(IdCliente);
  ```
- `IX_Polizas_TipoPoliza` (NONCLUSTERED) - Búsqueda por tipo:
  ```sql
  CREATE INDEX IX_Polizas_TipoPoliza ON Polizas(TipoPoliza);
  ```
- `IX_Polizas_Estatus` (NONCLUSTERED) - Búsqueda por estatus:
  ```sql
  CREATE INDEX IX_Polizas_Estatus ON Polizas(Estatus);
  ```
- `IX_Polizas_Fechas` (NONCLUSTERED) - Búsqueda por fechas de vigencia:
  ```sql
  CREATE INDEX IX_Polizas_Fechas ON Polizas(FechaInicio, FechaFin);
  ```

**Características:**
- Si se elimina un cliente, sus pólizas se eliminan en cascada
- El `Monto` usa DECIMAL(18,2) para precisión en valores monetarios
- Las fechas son de tipo DATE (sin hora)

#### 5.3.4. Diagrama de Relaciones - InsuranceDB

```
┌─────────────────────┐
│      Clientes       │
├─────────────────────┤
│ Id (PK)             │
│ NumeroIdentificacion│
│ Nombre              │
│ ApPaterno           │
│ ApMaterno           │
│ Telefono            │
│ Email               │
│ Direccion           │
│ FechaCreacion       │
│ FechaActualizacion  │
│ UserId              │
└──────────┬──────────┘
           │
           │ (1:N)
           │
           ▼
┌─────────────────────┐
│      Polizas        │
├─────────────────────┤
│ Id (PK)             │
│ IdCliente (FK)      │◄───┘
│ TipoPoliza          │
│ FechaInicio         │
│ FechaFin            │
│ Monto               │
│ Estatus             │
│ FechaCreacion       │
└─────────────────────┘
```

### 5.4. Scripts de Creación

#### 5.4.1. Script: 01_SCRIPT CREATE BASES.sql

```sql
-- Crear base de datos AuthDB si no existe
IF DB_ID('AuthDB') IS NULL
BEGIN
    CREATE DATABASE AuthDB;
END
GO

-- Crear base de datos InsuranceDB si no existe
IF DB_ID('InsuranceDB') IS NULL
BEGIN
    CREATE DATABASE InsuranceDB;
END
GO
```

#### 5.4.2. Script: 02_SCRIPT CREATE TABLAS AUTH.sql

Ver sección 5.2 para estructura completa de tablas.

#### 5.4.3. Script: 03_SCRIPT CREATE TABLAS INSURANCE.sql

Ver sección 5.3 para estructura completa de tablas.

---

## 6. APIs y Endpoints

### 6.1. Auth API

**Base URL Producción**: `https://auth-api-hscra6bsgahsepdb.westus2-01.azurewebsites.net`  
**Base URL Local**: `http://localhost:5001`  
**Swagger**: Disponible en la raíz (`/`)

#### 6.1.1. Autenticación

##### POST /api/auth/login

**Descripción**: Iniciar sesión y obtener tokens JWT

**Autenticación**: No requerida

**Request Body:**
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "expiresAt": "2026-01-16T19:00:00Z",
    "user": {
      "id": 1,
      "username": "admin",
      "email": "admin@example.com",
      "status": 1,
      "fechaCreacion": "2026-01-16T10:00:00Z",
      "roles": [
        {
          "id": 1,
          "nombre": "ADMINISTRADOR",
          "descripcion": "ADMINISTRADOR GENERAL",
          "status": 1
        }
      ]
    }
  },
  "message": null,
  "errors": []
}
```

**Response 401 Unauthorized:**
```json
{
  "success": false,
  "data": null,
  "message": "Credenciales inválidas",
  "errors": []
}
```

##### POST /api/auth/register

**Descripción**: Registrar nuevo usuario en el sistema

**Autenticación**: No requerida

**Request Body:**
```json
{
  "username": "nuevo_usuario",
  "email": "usuario@example.com",
  "password": "Password123!",
  "roleId": 2
}
```

**Validaciones:**
- `username`: Requerido, máximo 50 caracteres
- `email`: Requerido, formato válido, máximo 100 caracteres
- `password`: Requerido, mínimo 6 caracteres
- `roleId`: Requerido (1 = ADMINISTRADOR, 2 = CLIENTE)

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "id": 2,
    "username": "nuevo_usuario",
    "email": "usuario@example.com",
    "status": 1,
    "fechaCreacion": "2026-01-16T18:00:00Z",
    "roles": [...]
  },
  "message": null,
  "errors": []
}
```

##### POST /api/auth/refresh

**Descripción**: Refrescar token de acceso usando refresh token

**Autenticación**: No requerida

**Request Body:**
```json
"refresh_token_string"
```
*(String en el body, no objeto JSON)*

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "accessToken": "new_access_token...",
    "refreshToken": "new_refresh_token...",
    "expiresAt": "2026-01-16T20:00:00Z"
  },
  "message": null,
  "errors": []
}
```

##### POST /api/auth/logout

**Descripción**: Cerrar sesión y revocar refresh token

**Autenticación**: No requerida

**Request Body:**
```json
"refresh_token_string"
```

**Response 200 OK:**
```json
{
  "success": true,
  "data": true,
  "message": null,
  "errors": []
}
```

#### 6.1.2. Health Check

##### GET /health

**Descripción**: Verificar el estado del servicio

**Response:**
```
Healthy
```

#### 6.1.3. Swagger

##### GET / (o /swagger)

**Descripción**: Interfaz Swagger UI para documentación y pruebas de API

---

### 6.2. Seguros API

**Base URL Producción**: `https://seguros-api-allr-hycycshxhsgah8eq.westus2-01.azurewebsites.net`  
**Base URL Local**: `http://localhost:5002`  
**Swagger**: Disponible en `/swagger`

#### 6.2.1. Clientes

##### GET /api/Clientes

**Descripción**: Obtener todos los clientes del sistema

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Headers:**
```
Authorization: Bearer {access_token}
```

**Response 200 OK:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "numeroIdentificacion": "1234567890",
      "nombre": "Juan",
      "apPaterno": "Pérez",
      "apMaterno": "García",
      "telefono": "5551234567",
      "email": "juan@example.com",
      "direccion": "Calle 123, Ciudad",
      "fechaCreacion": "2026-01-16T10:00:00Z",
      "fechaActualizacion": null,
      "nombreCompleto": "Juan Pérez García"
    }
  ],
  "message": null,
  "errors": []
}
```

##### GET /api/Clientes/{id}

**Descripción**: Obtener cliente por ID

**Autenticación**: ✅ Requerida  
**Rol Requerido**: Cualquiera (ADMINISTRADOR o CLIENTE)

**Parameters:**
- `id` (int): ID del cliente

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "numeroIdentificacion": "1234567890",
    "nombre": "Juan",
    "apPaterno": "Pérez",
    "apMaterno": "García",
    "telefono": "5551234567",
    "email": "juan@example.com",
    "direccion": "Calle 123, Ciudad",
    "fechaCreacion": "2026-01-16T10:00:00Z",
    "fechaActualizacion": null,
    "nombreCompleto": "Juan Pérez García"
  },
  "message": null,
  "errors": []
}
```

##### GET /api/Clientes/User/{userId}

**Descripción**: Obtener cliente por UserID (relación con Auth)

**Autenticación**: ✅ Requerida  
**Rol Requerido**: Cualquiera

**Parameters:**
- `userId` (int): ID del usuario (Auth)

**Response 200 OK:** (mismo formato que GET /api/Clientes/{id})

##### GET /api/Clientes/identificacion/{numeroIdentificacion}

**Descripción**: Buscar cliente por número de identificación

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Parameters:**
- `numeroIdentificacion` (string): Número de identificación (10 dígitos)

**Response 200 OK:** (mismo formato que GET /api/Clientes/{id})

##### POST /api/Clientes

**Descripción**: Crear nuevo cliente

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Request Body:**
```json
{
  "numeroIdentificacion": "1234567890",
  "nombre": "Juan",
  "apPaterno": "Pérez",
  "apMaterno": "García",
  "telefono": "5551234567",
  "email": "juan@example.com",
  "direccion": "Calle 123, Ciudad"
}
```

**Validaciones:**
- `numeroIdentificacion`: Requerido, exactamente 10 dígitos numéricos
- `nombre`: Requerido, máximo 40 caracteres, solo letras
- `apPaterno`: Requerido, máximo 40 caracteres, solo letras
- `apMaterno`: Requerido, máximo 40 caracteres, solo letras
- `telefono`: Requerido, máximo 20 caracteres, formato válido
- `email`: Requerido, formato válido, máximo 100 caracteres
- `direccion`: Opcional, máximo 250 caracteres

**Response 201 Created:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "numeroIdentificacion": "1234567890",
    ...
  },
  "message": null,
  "errors": []
}
```

##### PUT /api/Clientes/{id}

**Descripción**: Actualizar cliente existente

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Parameters:**
- `id` (int): ID del cliente a actualizar

**Request Body:** (igual que POST /api/Clientes)

**Response 200 OK:** (mismo formato que POST)

##### PATCH /api/Clientes/UpdateMyInfo

**Descripción**: Actualizar información personal (solo dirección y teléfono)

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `CLIENTE`

**Request Body:**
```json
{
  "idCliente": 1,
  "direccion": "Nueva dirección 456",
  "telefono": "5559876543"
}
```

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "direccion": "Nueva dirección 456",
    "telefono": "5559876543",
    ...
  },
  "message": null,
  "errors": []
}
```

##### DELETE /api/Clientes/{id}

**Descripción**: Eliminar cliente

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Parameters:**
- `id` (int): ID del cliente a eliminar

**Response 200 OK:**
```json
{
  "success": true,
  "data": true,
  "message": null,
  "errors": []
}
```

#### 6.2.2. Pólizas

##### GET /api/Polizas/{id}

**Descripción**: Obtener póliza por ID

**Autenticación**: ✅ Requerida  
**Rol Requerido**: Cualquiera

**Parameters:**
- `id` (int): ID de la póliza

**Response 200 OK:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "idCliente": 1,
    "tipoPoliza": 2,
    "fechaInicio": "2026-01-01",
    "fechaFin": "2027-01-01",
    "monto": 50000.00,
    "estatus": 1,
    "fechaCreacion": "2026-01-16T10:00:00Z",
    "esVigente": true,
    "cliente": {
      "id": 1,
      "nombreCompleto": "Juan Pérez García",
      ...
    }
  },
  "message": null,
  "errors": []
}
```

##### GET /api/Polizas/cliente/{clienteId}

**Descripción**: Obtener todas las pólizas de un cliente

**Autenticación**: ✅ Requerida  
**Rol Requerido**: Cualquiera

**Parameters:**
- `clienteId` (int): ID del cliente

**Response 200 OK:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "idCliente": 1,
      "tipoPoliza": 2,
      ...
    }
  ],
  "message": null,
  "errors": []
}
```

##### GET /api/Polizas/tipo/{tipo}

**Descripción**: Filtrar pólizas por tipo

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Parameters:**
- `tipo` (int): Tipo de póliza (1=Vida, 2=Auto, 3=Hogar, 4=Salud)

**Response 200 OK:** (lista de pólizas)

##### GET /api/Polizas/estatus/{estatus}

**Descripción**: Filtrar pólizas por estatus

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Parameters:**
- `estatus` (int): Estatus (1=Vigente, 2=Vencida, 3=Cancelada, 4=Suspendida)

**Response 200 OK:** (lista de pólizas)

##### GET /api/Polizas/vigentes

**Descripción**: Obtener todas las pólizas vigentes

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Response 200 OK:** (lista de pólizas vigentes)

##### POST /api/Polizas

**Descripción**: Crear nueva póliza

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `ADMINISTRADOR`

**Request Body:**
```json
{
  "idCliente": 1,
  "tipoPoliza": 2,
  "fechaInicio": "2026-01-01",
  "fechaFin": "2027-01-01",
  "monto": 50000.00,
  "estatus": 1
}
```

**Validaciones:**
- `idCliente`: Requerido, debe existir
- `tipoPoliza`: Requerido (1-4)
- `fechaInicio`: Requerido, fecha válida
- `fechaFin`: Requerido, debe ser posterior a fechaInicio
- `monto`: Requerido, valor positivo mayor a 0.01
- `estatus`: Requerido (1-4)

**Response 201 Created:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "idCliente": 1,
    "tipoPoliza": 2,
    ...
  },
  "message": null,
  "errors": []
}
```

##### POST /api/Polizas/{id}/cancelar

**Descripción**: Cancelar póliza (solo clientes pueden cancelar sus propias pólizas)

**Autenticación**: ✅ Requerida  
**Rol Requerido**: `CLIENTE`

**Parameters:**
- `id` (int): ID de la póliza a cancelar

**Response 200 OK:**
```json
{
  "success": true,
  "data": true,
  "message": null,
  "errors": []
}
```

#### 6.2.3. Health Check

##### GET /health

**Descripción**: Verificar el estado del servicio

**Response:**
```
Healthy
```

---

## 7. DTOs y Modelos

### 7.1. Auth API - DTOs

#### 7.1.1. LoginDto

```csharp
public class LoginDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}
```

#### 7.1.2. RegisterDto

```csharp
public class RegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    public int RoleId { get; set; }
}
```

#### 7.1.3. LoginResponseDto

```csharp
public class LoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; }
}
```

#### 7.1.4. UserDto

```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<RoleDto> Roles { get; set; } = new List<RoleDto>();
}
```

#### 7.1.5. RoleDto

```csharp
public class RoleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public UserStatus Status { get; set; }
}
```

#### 7.1.6. TokenDto

```csharp
public class TokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### 7.2. Seguros API - DTOs

#### 7.2.1. ClienteDto

```csharp
public class ClienteDto
{
    public int Id { get; set; }
    public string NumeroIdentificacion { get; set; }
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string NombreCompleto { get; set; }
}
```

#### 7.2.2. CreateClienteDto

```csharp
public class CreateClienteDto
{
    [Required(ErrorMessage = "El número de identificación es obligatorio")]
    [StringLength(10, MinimumLength = 10)]
    [RegularExpression(@"^\d{10}$")]
    public string NumeroIdentificacion { get; set; }
    
    [Required]
    [MaxLength(40)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")]
    public string Nombre { get; set; }
    
    [Required]
    [MaxLength(40)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")]
    public string ApPaterno { get; set; }
    
    [Required]
    [MaxLength(40)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")]
    public string ApMaterno { get; set; }
    
    [Required]
    [MaxLength(20)]
    [Phone]
    public string Telefono { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [MaxLength(250)]
    public string Direccion { get; set; }
}
```

#### 7.2.3. UpdateClienteDto

```csharp
public class UpdateClienteDto
{
    public int IdCliente { get; set; }
    
    [MaxLength(250)]
    public string Direccion { get; set; }
    
    [MaxLength(20)]
    [Phone]
    public string Telefono { get; set; }
}
```

#### 7.2.4. PolizaDto

```csharp
public class PolizaDto
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public TipoPoliza TipoPoliza { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Monto { get; set; }
    public EstatusPoliza Estatus { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool EsVigente { get; set; }
    public ClienteDto Cliente { get; set; }
}
```

#### 7.2.5. CreatePolizaDto

```csharp
public class CreatePolizaDto
{
    [Required]
    public int IdCliente { get; set; }
    
    [Required]
    public TipoPoliza TipoPoliza { get; set; }
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Monto { get; set; }
    
    [Required]
    public EstatusPoliza Estatus { get; set; }
}
```

### 7.3. Clases Base

#### 7.3.1. BaseResponse<T>

```csharp
public class BaseResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
```

**Uso:** Todas las respuestas de la API utilizan este formato estándar.

---

## 8. Commands y Queries (CQRS)

### 8.1. Auth API

#### 8.1.1. Commands (Escritura)

##### LoginCommand
```csharp
public class LoginCommand : IRequest<BaseResponse<LoginResponseDto>>
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

##### RegisterCommand
```csharp
public class RegisterCommand : IRequest<BaseResponse<UserDto>>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
}
```

##### RefreshTokenCommand
```csharp
public class RefreshTokenCommand : IRequest<BaseResponse<TokenDto>>
{
    public string RefreshToken { get; set; }
}
```

##### LogoutCommand
```csharp
public class LogoutCommand : IRequest<BaseResponse<bool>>
{
    public string RefreshToken { get; set; }
}
```

##### CreateUserCommand
```csharp
public class CreateUserCommand : IRequest<BaseResponse<UserDto>>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public List<int> RoleIds { get; set; } = new List<int>();
}
```

#### 8.1.2. Queries (Lectura)

##### GetUserByIdQuery
```csharp
public class GetUserByIdQuery : IRequest<BaseResponse<UserDto>>
{
    public int Id { get; set; }
}
```

##### GetUserByUsernameQuery
```csharp
public class GetUserByUsernameQuery : IRequest<BaseResponse<UserDto>>
{
    public string Username { get; set; }
}
```

##### GetAllUsersQuery
```csharp
public class GetAllUsersQuery : IRequest<BaseResponse<IEnumerable<UserDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

##### GetAllRolesQuery
```csharp
public class GetAllRolesQuery : IRequest<BaseResponse<IEnumerable<RoleDto>>>
{
}
```

##### GetUserRolesQuery
```csharp
public class GetUserRolesQuery : IRequest<BaseResponse<IEnumerable<RoleDto>>>
{
    public int UserId { get; set; }
}
```

### 8.2. Seguros API

#### 8.2.1. Commands - Clientes

##### CreateClienteCommand
```csharp
public class CreateClienteCommand : IRequest<BaseResponse<ClienteDto>>
{
    public string NumeroIdentificacion { get; set; }
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public int? UserId { get; set; }
}
```

##### UpdateClienteCommand
```csharp
public class UpdateClienteCommand : IRequest<BaseResponse<ClienteDto>>
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
}
```

##### DeleteClienteCommand
```csharp
public class DeleteClienteCommand : IRequest<BaseResponse<bool>>
{
    public int Id { get; set; }
}
```

#### 8.2.2. Queries - Clientes

##### GetClienteByIdQuery
```csharp
public class GetClienteByIdQuery : IRequest<BaseResponse<ClienteDto>>
{
    public int Id { get; set; }
}
```

##### GetClienteByUserIdQuery
```csharp
public class GetClienteByUserIdQuery : IRequest<BaseResponse<ClienteDto>>
{
    public int userId { get; set; }
}
```

##### GetClienteByIdentificacionQuery
```csharp
public class GetClienteByIdentificacionQuery : IRequest<BaseResponse<ClienteDto>>
{
    public string NumeroIdentificacion { get; set; }
}
```

##### GetAllClientesQuery
```csharp
public class GetAllClientesQuery : IRequest<BaseResponse<IEnumerable<ClienteDto>>>
{
}
```

#### 8.2.3. Commands - Pólizas

##### CreatePolizaCommand
```csharp
public class CreatePolizaCommand : IRequest<BaseResponse<PolizaDto>>
{
    public int IdCliente { get; set; }
    public TipoPoliza TipoPoliza { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Monto { get; set; }
    public EstatusPoliza Estatus { get; set; }
}
```

##### UpdatePolizaCommand
```csharp
public class UpdatePolizaCommand : IRequest<BaseResponse<PolizaDto>>
{
    public int Id { get; set; }
    public DateTime? FechaFin { get; set; }
    public decimal? Monto { get; set; }
    public EstatusPoliza? Estatus { get; set; }
}
```

##### CancelarPolizaCommand
```csharp
public class CancelarPolizaCommand : IRequest<BaseResponse<bool>>
{
    public int Id { get; set; }
    public int UserId { get; set; }
}
```

#### 8.2.4. Queries - Pólizas

##### GetPolizaByIdQuery
```csharp
public class GetPolizaByIdQuery : IRequest<BaseResponse<PolizaDto>>
{
    public int Id { get; set; }
}
```

##### GetPolizasByClienteQuery
```csharp
public class GetPolizasByClienteQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
{
    public int ClienteId { get; set; }
}
```

##### GetPolizasByTipoQuery
```csharp
public class GetPolizasByTipoQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
{
    public TipoPoliza TipoPoliza { get; set; }
}
```

##### GetPolizasByEstatusQuery
```csharp
public class GetPolizasByEstatusQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
{
    public EstatusPoliza Estatus { get; set; }
}
```

##### GetPolizasVigentesQuery
```csharp
public class GetPolizasVigentesQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
{
}
```

##### GetPolizasByFechasQuery
```csharp
public class GetPolizasByFechasQuery : IRequest<BaseResponse<IEnumerable<PolizaDto>>>
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
```

---

## 9. Seguridad y Autenticación

### 9.1. JWT (JSON Web Tokens)

#### 9.1.1. Configuración

- **Algoritmo**: HS256 (HMAC-SHA256)
- **Secret Key**: Mínimo 32 caracteres (256 bits) - **IMPORTANTE**
- **Issuer**: `Auth.API`
- **Audience**: `Seguros.Microservicios`
- **Expiración**: Configurable (por defecto 1 hora)

#### 9.1.2. Claims del Token

Los tokens JWT incluyen los siguientes claims:

- `nameid`: ID del usuario (UserId)
- `unique_name`: Username
- `email`: Email del usuario
- `role`: Rol del usuario (ADMINISTRADOR o CLIENTE)
- `nbf`: Not Before (tiempo de validez)
- `exp`: Expiration Time (expiración)
- `iat`: Issued At (tiempo de emisión)
- `iss`: Issuer (Auth.API)
- `aud`: Audience (Seguros.Microservicios)

#### 9.1.3. Validación de Tokens

En **Seguros API**, los tokens se validan contra la configuración JWT:
- Verificación de firma (HS256)
- Validación de expiración
- Validación de issuer
- Validación de audience
- Validación de lifetime

#### 9.1.4. Refresh Tokens

- **Almacenamiento**: Base de datos `AuthDB.RefreshTokens`
- **Expiración**: Configurable (por defecto 7 días)
- **Revocación**: Los tokens pueden ser revocados antes de expirar
- **Uso**: Renovar access tokens sin re-autenticación

### 9.2. Hash de Contraseñas

- **Algoritmo**: BCrypt.Net-Next
- **Work Factor**: Determinado automáticamente por BCrypt
- **Salt**: Generado automáticamente por BCrypt
- **Almacenamiento**: Solo hash, nunca contraseñas en texto plano

### 9.3. Roles y Permisos

#### 9.3.1. Roles Disponibles

| ID | Nombre | Descripción | Permisos |
|----|--------|-------------|----------|
| 1 | `ADMINISTRADOR` | Administrador general | Acceso completo a todas las funcionalidades |
| 2 | `CLIENTE` | Cliente del sistema | Acceso limitado a su propia información |

#### 9.3.2. Autorización por Endpoint

**Auth API:**
- Todos los endpoints son públicos (sin autenticación requerida)

**Seguros API:**
- Todos los endpoints requieren autenticación (`[Authorize]`)
- Endpoints específicos requieren rol `ADMINISTRADOR` (`[Authorize(Roles = "ADMINISTRADOR")]`)
- Algunos endpoints permiten rol `CLIENTE` (`[Authorize(Roles = "CLIENTE")]`)

### 9.4. CORS (Cross-Origin Resource Sharing)

**Desarrollo/Docker:**
- Policy: `AllowAll` - Permite cualquier origen

**Producción:**
- Policy: `Production` - Origen específico configurado

### 9.5. Validación de Input

- **Data Annotations**: Validaciones en DTOs y entidades
- **FluentValidation**: Validaciones complejas (Auth.Application)
- **Validación en BD**: Constraints CHECK en SQL Server

---

## 10. Cache y Performance

### 10.1. Redis Cache (Solo Seguros API)

#### 10.1.1. Configuración

- **Tecnología**: Redis 7-alpine
- **Propósito**: Cache distribuido para consultas frecuentes
- **Instancia**: `seguros-redis-allr.redis.cache.windows.net:6380` (Azure)

#### 10.1.2. Estrategia de Cache

**Patrón**: Cache-Aside (Lazy Loading)

1. **Query Handler:**
   - Intenta obtener del cache
   - Si no existe (cache miss), consulta la BD
   - Almacena resultado en cache
   - Retorna resultado

2. **Command Handler:**
   - Ejecuta la operación en BD
   - Invalida claves relacionadas en cache
   - Retorna resultado

#### 10.1.3. Claves de Cache

**Formato:** `{prefijo}:{tipo}:{identificador}`

**Claves de Clientes:**
- `Seguros:cliente:id:{id}` - Cliente por ID
- `Seguros:cliente:userid:{userId}` - Cliente por UserID
- `Seguros:cliente:identificacion:{numero}` - Cliente por identificación
- `Seguros:clientes:all` - Lista de todos los clientes

**Claves de Pólizas (Futuro):**
- `Seguros:poliza:id:{id}` - Póliza por ID
- `Seguros:polizas:cliente:{clienteId}` - Pólizas por cliente
- `Seguros:polizas:all` - Lista de todas las pólizas

#### 10.1.4. Tiempos de Expiración

| Tipo | Configuración | Tiempo |
|------|---------------|--------|
| Default | `CacheSettings__DefaultExpirationMinutes` | 5 minutos |
| Clientes | `CacheSettings__ClienteExpirationMinutes` | 10 minutos |
| Pólizas | `CacheSettings__PolizaExpirationMinutes` | 15 minutos |

#### 10.1.5. Invalidación de Cache

Los siguientes comandos invalidan cache automáticamente:

- **CreateClienteCommand**: Invalida `clientes:all`
- **UpdateClienteCommand**: Invalida todas las claves del cliente
- **DeleteClienteCommand**: Invalida todas las claves del cliente
- **CreatePolizaCommand**: (Futuro) Invalida claves de pólizas
- **CancelarPolizaCommand**: (Futuro) Invalida claves de pólizas

#### 10.1.6. Fallback

Si Redis no está disponible:
- El sistema usa `IDistributedMemoryCache` como fallback
- Se registra un warning en los logs
- La aplicación continúa funcionando normalmente

---

## 11. Logging

### 11.1. Serilog

#### 11.1.1. Configuración

- **Framework**: Serilog 3.1.1 / Serilog.AspNetCore 8.0.0
- **Formato**: Logs estructurados con propiedades JSON
- **Niveles**: Information, Warning, Error, Fatal

#### 11.1.2. Destinos (Sinks)

**Consola:**
- Formato: `[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}`
- Nivel: Information

**Archivos:**
- Ubicación: `Logs/{Microservicio}-{YYYYMMdd}.txt`
- Formato: `[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}`
- Rotación: Por tamaño (configurable en MB)
- Límite de tamaño: Configurable (por defecto 10 MB)

#### 11.1.3. Propiedades Personalizadas

- `Microservicio`: Nombre del microservicio (Auth/Seguros)
- `RequestId`: ID de la petición HTTP
- `ConnectionId`: ID de conexión HTTP
- `RequestPath`: Ruta de la petición

#### 11.1.4. Logging de HTTP Requests

**Middleware**: `UseSerilogRequestLogging()`

**Template:**
```
HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms
```

**Niveles dinámicos:**
- Error: Si hay excepción
- Error: Si status code > 499
- Information: En otros casos

### 11.2. Eventos de JWT

**En Auth API:**
- `OnAuthenticationFailed`: Logs errores de autenticación
- `OnTokenValidated`: Logs de tokens validados exitosamente
- `OnChallenge`: Logs de desafíos de autenticación
- `OnMessageReceived`: Logs de recepción de tokens

### 11.3. Middleware de Logging

**Auth API:**
- `RequestLoggingMiddleware`: Logging personalizado de peticiones
- `ExceptionHandlingMiddleware`: Manejo centralizado de excepciones

**Seguros API:**
- `ExceptionHandlingMiddleware`: Manejo centralizado de excepciones

---

## 12. Configuración

### 12.1. Auth API - appsettings.json

```json
{
  "ConnectionStrings": {
    "AuthDB": "Server=sqlserver;Database=AuthDB;User Id=sa;Password=SQLsa736.;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "AuthSecretKey2026!@#$%^&*()_+",
    "Issuer": "Auth.API",
    "Audience": "Seguros.Microservicios",
    "ExpiryInHours": 1
  },
  "TamanhoMaximoLogEnMegas": 10,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 12.2. Seguros API - appsettings.json

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
  "TamanhoMaximoLogEnMegas": 10,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
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
  },
  "AllowedHosts": "*"
}
```

### 12.3. Azure App Service - Variables de Entorno

#### 12.3.1. Auth API

**Connection Strings:**
- `ConnectionStrings__AuthDB`: Cadena de conexión a SQL Server

**Application Settings:**
- `JwtSettings__SecretKey`: Clave secreta JWT (mínimo 32 caracteres)
- `JwtSettings__Issuer`: `Auth.API`
- `JwtSettings__Audience`: `Seguros.Microservicios`
- `JwtSettings__ExpiryInHours`: `1`
- `ASPNETCORE_ENVIRONMENT`: `Production`
- `TamanhoMaximoLogEnMegas`: `10`

#### 12.3.2. Seguros API

**Connection Strings:**
- `ConnectionStrings__InsuranceDB`: Cadena de conexión a SQL Server

**Application Settings:**
- `JwtSettings__SecretKey`: Clave secreta JWT (debe coincidir con Auth API)
- `JwtSettings__Issuer`: `Auth.API`
- `JwtSettings__Audience`: `Seguros.Microservicios`
- `AuthApiUrl`: `https://auth-api-hscra6bsgahsepdb.westus2-01.azurewebsites.net`
- `Redis__ConnectionString`: `seguros-redis-allr.redis.cache.windows.net:6380,password=...,ssl=True,abortConnect=False`
- `Redis__InstanceName`: `Seguros:`
- `CacheSettings__DefaultExpirationMinutes`: `5`
- `CacheSettings__ClienteExpirationMinutes`: `10`
- `CacheSettings__PolizaExpirationMinutes`: `15`
- `ASPNETCORE_ENVIRONMENT`: `Production`

---

## 13. Infraestructura y DevOps

### 13.1. Docker

#### 13.1.1. Dockerfile - Auth API

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet","Auth.API.dll"]
```

#### 13.1.2. Dockerfile - Seguros API

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet","Seguros.API.dll"]
```

### 13.2. Docker Compose

**Archivo**: `src/docker-compose.yml`

**Servicios:**
- `sqlserver`: SQL Server 2022
- `auth-api`: Microservicio Auth (puerto 5001)
- `seguros-api`: Microservicio Seguros (puerto 5002)
- `redis`: Redis 7-alpine

**Redes:**
- `backend`: Red interna para comunicación entre servicios

### 13.3. GitHub Actions - Workflows

#### 13.3.1. Auth API Workflow

**Archivo**: `.github/workflows/auth-api.yml`

**Trigger:**
- Push a `main` cuando cambian archivos en `src/Auth/**`

**Pasos:**
1. Checkout código
2. Setup .NET 8.0.x
3. Restore dependencias
4. Build proyecto
5. Publish proyecto
6. Verificar DLLs incluidos
7. Eliminar archivos de Seguros (si existen)
8. Deploy a Azure App Service

#### 13.3.2. Seguros API Workflow

**Archivo**: `.github/workflows/seguros-api.yml`

**Trigger:**
- Push a `main` cuando cambian archivos en `src/Seguros/**`

**Pasos:** (similar a Auth API)

### 13.4. Azure App Service

**Auth API:**
- **Nombre**: `auth-api`
- **Stack**: .NET 8
- **Región**: West US 2

**Seguros API:**
- **Nombre**: `seguros-api-allr`
- **Stack**: .NET 8
- **Región**: West US 2

---

## 14. Diagramas de Arquitectura

### 14.1. Arquitectura General del Sistema

```
┌──────────────────────────────────────────────────────────────┐
│                      Cliente/Aplicación                       │
│                     (Web, Mobile, etc.)                       │
└───────────────────────────┬──────────────────────────────────┘
                            │
                            │ HTTPS
                            ▼
            ┌───────────────────────────────┐
            │    Azure App Gateway/Load      │
            │         Balancer (HTTPS)       │
            └───────────────┬───────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        │                                       │
        ▼                                       ▼
┌──────────────────┐                  ┌──────────────────┐
│    Auth API      │                  │  Seguros API     │
│  Azure App Service│                  │  Azure App Service│
│  .NET 8.0        │                  │  .NET 8.0        │
└────────┬─────────┘                  └────────┬─────────┘
         │                                     │
         │                                     │
         │                                     │
         │                                     │
         │                                     │ ┌──────────┐
         │                                     ├─┤  Redis   │
         │                                     │ │  Cache   │
         │                                     │ └──────────┘
         │                                     │
         ▼                                     ▼
┌──────────────────┐                  ┌──────────────────┐
│    AuthDB        │                  │   InsuranceDB    │
│  Azure SQL DB    │                  │  Azure SQL DB    │
│  (Auth Tables)   │                  │  (Seguros Tables)│
└──────────────────┘                  └──────────────────┘
```

### 14.2. Flujo CQRS - Commands (Escritura)

```
┌──────────────┐
│  Controller  │
└──────┬───────┘
       │
       │ Command (IRequest)
       ▼
┌──────────────┐
│   MediatR    │
└──────┬───────┘
       │
       │ Route to Handler
       ▼
┌──────────────┐
│    Handler   │ ───┐
│ (Application)│    │
└──────┬───────┘    │
       │            │
       │            │ 1. Validate
       │            │ 2. Business Logic
       │            │ 3. Update Cache
       │            │
       │            └─► Repository
       │                  │
       │                  ▼
       │            ┌──────────────┐
       │            │ Unit of Work │
       │            │  (Dapper)    │
       │            └──────┬───────┘
       │                   │
       │                   ▼
       │            ┌──────────────┐
       │            │  SQL Server  │
       │            └──────────────┘
       │
       │ BaseResponse<T>
       ▼
┌──────────────┐
│  Controller  │
│  (Response)  │
└──────────────┘
```

### 14.3. Flujo CQRS - Queries (Lectura)

```
┌──────────────┐
│  Controller  │
└──────┬───────┘
       │
       │ Query (IRequest)
       ▼
┌──────────────┐
│   MediatR    │
└──────┬───────┘
       │
       ▼
┌──────────────┐
│    Handler   │
│ (Application)│
└──────┬───────┘
       │
       │ 1. Check Cache
       ▼
┌──────────────┐      ┌──────────────┐
│    Redis     │ YES  │    Cache     │
│    Cache     │◄─────┤    Hit?      │
└──────────────┘      └──────┬───────┘
                             │ NO
                             ▼
                      ┌──────────────┐
                      │  Repository  │
                      │   (Dapper)   │
                      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │  SQL Server  │
                      └──────┬───────┘
                             │
                             │ 2. Store in Cache
                             ▼
                      ┌──────────────┐
                      │    Redis     │
                      └──────────────┘
                             │
                             │ BaseResponse<T>
                             ▼
                      ┌──────────────┐
                      │  Controller  │
                      │  (Response)  │
                      └──────────────┘
```


**Ejemplo - Obtener Cliente por ID:**

```
GET /api/Clientes/{id}
    ↓
GetClienteByIdQuery
    ↓
GetClienteByIdQueryHandler
    ↓
1. Check Cache: "Seguros:cliente:id:{id}"
2. Si existe → Retornar desde cache
3. Si no existe:
   a. Consultar BD (Dapper)
   b. Almacenar en cache
   c. Retornar ClienteDto
```

### 14.4. Flujo de Autenticación Completo

```
┌──────────┐
│ Cliente  │
└────┬─────┘
     │
     │ 1. POST /api/auth/login
     │    { username, password }
     ▼
┌──────────────┐
│  Auth API    │
│ (Controller) │
└──────┬───────┘
       │
       │ 2. LoginCommand
       ▼
┌──────────────┐
│  LoginHandler│
│ (Application)│
└──────┬───────┘
       │
       │ 3. Validate Credentials
       ▼
┌──────────────┐      ┌──────────────┐
│  Repository  │◄─────┤  AuthDB      │
│  (Dapper)    │      │  Users Table │
└──────┬───────┘      └──────────────┘
       │
       │ 4. BCrypt.Verify(password, passwordHash)
       ▼
┌──────────────┐
│ TokenService │
│(Infrastructure)│
└──────┬───────┘
       │
       │ 5. Generate JWT Access Token
       │ 6. Generate Refresh Token
       │ 7. Store RefreshToken in AuthDB
       ▼
┌──────────────┐
│  Access Token│
│ Refresh Token│
└──────┬───────┘
       │
       │ 8. Return Tokens
       ▼
┌──────────┐
│ Cliente  │
│ (Tokens) │
└────┬─────┘
     │
     │ 9. Use Access Token
     │    Authorization: Bearer {token}
     ▼
┌──────────────┐
│ Seguros API  │
│ (Controller) │
└──────┬───────┘
       │
       │ 10. Validate JWT Token
       ▼
┌──────────────┐
│ JWT Validator│
│  (Middleware)│
└──────┬───────┘
       │
       │ 11. Extract Claims (userId, roles)
       ▼
┌──────────────┐
│  Endpoint    │
│  Execution   │
└──────────────┘
```

### 14.5. Flujo de Cache en Seguros API

```
┌─────────────────────────────────────────────────────────┐
│              GET /api/Clientes/{id}                      │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
        ┌────────────────────────┐
        │  Query Handler         │
        │  GetClienteByIdQuery   │
        └────────┬───────────────┘
                 │
                 │ 1. Generate Cache Key
                 ▼
        ┌────────────────────────┐
        │ Cache Key:             │
        │ "Seguros:cliente:id:1" │
        └────────┬───────────────┘
                 │
                 │ 2. Check Redis Cache
                 ▼
        ┌────────────────────────┐
        │     Cache Hit?         │
        └────┬───────────────┬───┘
             │ YES           │ NO
             │               │
             ▼               ▼
    ┌──────────────┐  ┌──────────────┐
    │ Return from  │  │ Query SQL    │
    │    Cache     │  │    Server    │
    │              │  └──────┬───────┘
    └──────────────┘         │
                             │ 3. Get Result
                             ▼
                    ┌──────────────┐
                    │ Store in     │
                    │ Redis Cache  │
                    │ (TTL: 10 min)│
                    └──────┬───────┘
                           │
                           │ 4. Return Result
                           ▼
                    ┌──────────────┐
                    │   Response   │
                    │  ClienteDto  │
                    └──────────────┘
```

---

## 15. Commands y Queries Completos

### 15.1. Auth API - Commands

#### 15.1.1. LoginCommand

**Propósito**: Autenticar usuario y obtener tokens

**Request:**
```csharp
public class LoginCommand : IRequest<BaseResponse<LoginResponseDto>>
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

**Response:**
```csharp
{
    "success": true,
    "data": {
        "accessToken": "eyJhbGci...",
        "refreshToken": "refresh_token_string",
        "expiresAt": "2026-01-16T19:00:00Z",
        "user": { ... }
    }
}
```

**Flujo:**
1. Validar credenciales contra AuthDB
2. Verificar hash con BCrypt
3. Generar JWT Access Token
4. Generar Refresh Token
5. Guardar RefreshToken en BD
6. Retornar tokens y usuario

#### 15.1.2. RegisterCommand

**Propósito**: Registrar nuevo usuario

**Request:**
```csharp
public class RegisterCommand : IRequest<BaseResponse<UserDto>>
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public int RoleId { get; set; }
}
```

**Flujo:**
1. Validar unicidad de username y email
2. Hash de contraseña con BCrypt
3. Crear usuario en AuthDB
4. Asignar rol (UserRoles)
5. Retornar usuario creado

#### 15.1.3. RefreshTokenCommand

**Propósito**: Renovar access token usando refresh token

**Request:**
```csharp
public class RefreshTokenCommand : IRequest<BaseResponse<TokenDto>>
{
    public string RefreshToken { get; set; }
}
```

**Flujo:**
1. Validar refresh token en BD
2. Verificar expiración y revocación
3. Generar nuevo access token
4. Opcionalmente generar nuevo refresh token
5. Retornar tokens

#### 15.1.4. LogoutCommand

**Propósito**: Cerrar sesión y revocar refresh token

**Request:**
```csharp
public class LogoutCommand : IRequest<BaseResponse<bool>>
{
    public string RefreshToken { get; set; }
}
```

**Flujo:**
1. Marcar refresh token como revocado
2. Retornar éxito

### 15.2. Auth API - Queries

#### 15.2.1. GetUserByIdQuery

**Propósito**: Obtener usuario por ID

**Request:**
```csharp
public class GetUserByIdQuery : IRequest<BaseResponse<UserDto>>
{
    public int Id { get; set; }
}
```

**Response:**
```csharp
{
    "success": true,
    "data": {
        "id": 1,
        "username": "admin",
        "email": "admin@example.com",
        "roles": [ ... ]
    }
}
```

#### 15.2.2. GetUserByUsernameQuery

**Propósito**: Obtener usuario por username

#### 15.2.3. GetAllUsersQuery

**Propósito**: Obtener lista paginada de usuarios

**Request:**
```csharp
public class GetAllUsersQuery : IRequest<BaseResponse<IEnumerable<UserDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

### 15.3. Seguros API - Commands (Clientes)

#### 15.3.1. CreateClienteCommand

**Propósito**: Crear nuevo cliente

**Request:**
```csharp
public class CreateClienteCommand : IRequest<BaseResponse<ClienteDto>>
{
    public string NumeroIdentificacion { get; set; }
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public int? UserId { get; set; }
}
```

**Validaciones:**
- `NumeroIdentificacion`: 10 dígitos numéricos, único
- `Nombre`, `ApPaterno`, `ApMaterno`: Solo letras, max 40 caracteres
- `Email`: Formato válido, único
- `Telefono`: Formato válido, max 20 caracteres

**Flujo:**
1. Validar datos
2. Verificar unicidad de `NumeroIdentificacion`
3. Insertar en InsuranceDB
4. Invalidar cache de clientes
5. Retornar ClienteDto

#### 15.3.2. UpdateClienteCommand

**Propósito**: Actualizar cliente existente

**Flujo similar a Create, pero con UPDATE en BD**

#### 15.3.3. DeleteClienteCommand

**Propósito**: Eliminar cliente

**Flujo:**
1. Verificar existencia
2. Eliminar en cascada pólizas (FK constraint)
3. Eliminar cliente
4. Invalidar cache de clientes y pólizas
5. Retornar éxito

### 15.4. Seguros API - Queries (Clientes)

#### 15.4.1. GetClienteByIdQuery

**Propósito**: Obtener cliente por ID

**Cache Key**: `Seguros:cliente:id:{id}`  
**TTL**: 10 minutos

#### 15.4.2. GetClienteByUserIdQuery

**Propósito**: Obtener cliente por UserID (relación con Auth)

**Cache Key**: `Seguros:cliente:userid:{userId}`  
**TTL**: 10 minutos

#### 15.4.3. GetAllClientesQuery

**Propósito**: Obtener todos los clientes

**Cache Key**: `Seguros:clientes:all`  
**TTL**: 10 minutos

### 15.5. Seguros API - Commands (Pólizas)

#### 15.5.1. CreatePolizaCommand

**Propósito**: Crear nueva póliza

**Request:**
```csharp
public class CreatePolizaCommand : IRequest<BaseResponse<PolizaDto>>
{
    public int IdCliente { get; set; }
    public TipoPoliza TipoPoliza { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Monto { get; set; }
    public EstatusPoliza Estatus { get; set; }
}
```

**Validaciones:**
- `FechaFin` > `FechaInicio`
- `Monto` > 0.01
- `IdCliente` debe existir

**Flujo:**
1. Validar cliente existe
2. Validar fechas
3. Insertar en InsuranceDB
4. Invalidar cache de pólizas
5. Retornar PolizaDto

#### 15.5.2. CancelarPolizaCommand

**Propósito**: Cancelar póliza (solo CLIENTE)

**Request:**
```csharp
public class CancelarPolizaCommand : IRequest<BaseResponse<bool>>
{
    public int Id { get; set; }
    public int UserId { get; set; }
}
```

**Flujo:**
1. Verificar póliza existe y pertenece al cliente
2. Actualizar `Estatus` = Cancelada
3. Invalidar cache
4. Retornar éxito

### 15.6. Seguros API - Queries (Pólizas)

#### 15.6.1. GetPolizaByIdQuery

**Cache Key**: `Seguros:poliza:id:{id}`  
**TTL**: 5 minutos

#### 15.6.2. GetPolizasByClienteQuery

**Cache Key**: `Seguros:polizas:cliente:{clienteId}`  
**TTL**: 5 minutos

#### 15.6.3. GetPolizasByTipoQuery

**Cache Key**: `Seguros:polizas:tipo:{tipo}`  
**TTL**: 5 minutos

#### 15.6.4. GetPolizasVigentesQuery

**Cache Key**: `Seguros:polizas:vigentes`  
**TTL**: 5 minutos

---

## 16. DTOs y Modelos Completos

### 16.1. Auth API - DTOs

#### 16.1.1. LoginDto

```csharp
public class LoginDto
{
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
}
```

#### 16.1.2. RegisterDto

```csharp
public class RegisterDto
{
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public string Password { get; set; }

    [Required]
    public int RoleId { get; set; }
}
```

#### 16.1.3. LoginResponseDto

```csharp
public class LoginResponseDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; }
}
```

#### 16.1.4. UserDto

```csharp
public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public DateTime FechaCreacion { get; set; }
    public List<RoleDto> Roles { get; set; }
}
```

#### 16.1.5. RoleDto

```csharp
public class RoleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public UserStatus Status { get; set; }
}
```

#### 16.1.6. TokenDto

```csharp
public class TokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

### 16.2. Seguros API - DTOs

#### 16.2.1. ClienteDto

```csharp
public class ClienteDto
{
    public int Id { get; set; }
    public string NumeroIdentificacion { get; set; }
    public string Nombre { get; set; }
    public string ApPaterno { get; set; }
    public string ApMaterno { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }
    public string Direccion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string NombreCompleto { get; set; }
}
```

#### 16.2.2. CreateClienteDto

```csharp
public class CreateClienteDto
{
    [Required]
    [StringLength(10, MinimumLength = 10)]
    [RegularExpression(@"^\d{10}$")]
    public string NumeroIdentificacion { get; set; }
    
    [Required]
    [MaxLength(40)]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$")]
    public string Nombre { get; set; }
    
    // ... otros campos con validaciones similares
}
```

#### 16.2.3. UpdateMyInfoDto

```csharp
public class UpdateMyInfoDto
{
    public int IdCliente { get; set; }
    
    [MaxLength(250)]
    public string Direccion { get; set; }
    
    [MaxLength(20)]
    [Phone]
    public string Telefono { get; set; }
}
```

#### 16.2.4. PolizaDto

```csharp
public class PolizaDto
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public TipoPoliza TipoPoliza { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Monto { get; set; }
    public EstatusPoliza Estatus { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool EsVigente { get; set; }
    public ClienteDto Cliente { get; set; }
}
```

#### 16.2.5. CreatePolizaDto

```csharp
public class CreatePolizaDto
{
    [Required]
    public int IdCliente { get; set; }
    
    [Required]
    public TipoPoliza TipoPoliza { get; set; }
    
    [Required]
    public DateTime FechaInicio { get; set; }
    
    [Required]
    public DateTime FechaFin { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Monto { get; set; }
    
    [Required]
    public EstatusPoliza Estatus { get; set; }
}
```

### 16.3. BaseResponse<T>

**Clase base para todas las respuestas:**

```csharp
public class BaseResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
```

**Ejemplo de uso:**
```csharp
{
    "success": true,
    "data": { ... },
    "message": null,
    "errors": []
}
```

---

## 17. Enumeraciones (Enums)

### 17.1. Auth Enums

#### 17.1.1. UserStatus

```csharp
public enum UserStatus
{
    Inactivo = 0,
    Activo = 1
}
```

#### 17.1.2. RoleType

```csharp
// Valores en BD:
// 1 = ADMINISTRADOR
// 2 = CLIENTE
```

#### 17.1.3. TokenType

```csharp
// Para RefreshTokens
// No hay enum explícito, se usa string
```

### 17.2. Seguros Enums

#### 17.2.1. TipoPoliza

```csharp
public enum TipoPoliza
{
    Vida = 1,
    Auto = 2,
    Hogar = 3,
    Salud = 4
}
```

#### 17.2.2. EstatusPoliza

```csharp
public enum EstatusPoliza
{
    Vigente = 1,
    Vencida = 2,
    Cancelada = 3,
    Suspendida = 4
}
```

---

## 18. Middleware Personalizado

### 18.1. ExceptionHandlingMiddleware

**Propósito**: Manejo centralizado de excepciones

**Funcionalidad:**
- Captura todas las excepciones no manejadas
- Logging de errores con Serilog
- Respuesta HTTP estándar con `BaseResponse<T>`
- Códigos de estado apropiados (400, 500, etc.)

**Ubicación**: `Auth.Infrastructure/Middleware` y `Seguros.Infrastructure/Middleware`

### 18.2. RequestLoggingMiddleware (Solo Auth API)

**Propósito**: Logging detallado de peticiones HTTP

**Funcionalidad:**
- Log de inicio de petición
- Log de finalización con duración
- Información de token (si existe)
- Propiedades personalizadas: RequestPath, RequestId, ConnectionId

**Ubicación**: `Auth.Infrastructure/Middleware`

---

## 19. Health Checks

### 19.1. Configuración

**Ambos microservicios incluyen health checks para SQL Server:**

```csharp
services.AddHealthChecks()
    .AddSqlServer(
        connectionString,
        name: "sqlserver",
        tags: new[] { "db", "sql", "sqlserver" });
```

### 19.2. Endpoint

**Ruta**: `GET /health`

**Response:**
```
Healthy
```

**Uso**: Azure App Service usa este endpoint para verificar que la aplicación está funcionando.

---

## 20. Consideraciones Finales

### 20.1. Performance

- **Cache Redis**: Reduce consultas a BD en Seguros API
- **Dapper**: Micro ORM más rápido que EF Core para consultas
- **Connection Pooling**: SQL Server connection pooling habilitado
- **Async/Await**: Todas las operaciones I/O son asíncronas

### 20.2. Escalabilidad

- **Microservicios**: Independientes y escalables horizontalmente
- **Stateless**: APIs sin estado, fáciles de escalar
- **Cache Distribuido**: Redis permite compartir cache entre instancias
- **Load Balancer**: Azure App Gateway distribuye carga

### 20.3. Mantenibilidad

- **Clean Architecture**: Separación clara de responsabilidades
- **CQRS**: Separación de lectura y escritura
- **Dependency Injection**: Facilita testing y cambios
- **Logging Estructurado**: Facilita debugging y monitoreo

### 20.4. Seguridad

- **JWT**: Tokens seguros con expiración
- **HTTPS**: Obligatorio en producción
- **Hash BCrypt**: Contraseñas nunca en texto plano
- **Validación**: Input validado en múltiples capas
- **CORS**: Configurado según entorno

### 20.5. Testing

- **Unit Tests**: Handlers, Services, Repositories
- **Integration Tests**: Endpoints completos
- **Mocking**: MediatR y Dapper se pueden mockear fácilmente

---

## 21. Referencias y Documentación Adicional

### 21.1. Documentación Oficial

- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [MediatR GitHub](https://github.com/jbogard/MediatR)
- [Dapper Documentation](https://github.com/DapperLib/Dapper)
- [Serilog Documentation](https://serilog.net/)
- [Swagger/OpenAPI](https://swagger.io/)
- [JWT.io](https://jwt.io/)
- [Redis Documentation](https://redis.io/docs/)

### 21.2. Arquitectura y Patrones

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Mediator Pattern](https://refactoring.guru/design-patterns/mediator)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

### 21.3. Herramientas

- [Azure App Service](https://azure.microsoft.com/en-us/services/app-service/)
- [Azure SQL Database](https://azure.microsoft.com/en-us/services/sql-database/)
- [Azure Redis Cache](https://azure.microsoft.com/en-us/services/cache/)
- [GitHub Actions](https://github.com/features/actions)
- [Docker Documentation](https://docs.docker.com/)

---
