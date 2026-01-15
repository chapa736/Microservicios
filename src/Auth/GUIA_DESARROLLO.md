# Guía: Cómo Agregar Nuevas Funcionalidades

## Ejemplo: Agregar funcionalidad "Cambiar Contraseña"

### 1. Core - Crear Command (Finasist.Auth.Core/Commands/)

```csharp
public class ChangePasswordCommand : IRequest<BaseResponse<bool>>
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}
```

### 2. Core - Crear DTO si es necesario (Finasist.Auth.Core/DTOs/)

```csharp
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; }
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; }
}
```

### 3. Application - Crear Handler (Finasist.Auth.Application/Handlers/)

```csharp
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, BaseResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId);
            
            if (user == null)
            {
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Usuario no encontrado"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return new BaseResponse<bool>
                {
                    Success = false,
                    Message = "Contraseña actual incorrecta"
                };
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new BaseResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Contraseña actualizada exitosamente"
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<bool>
            {
                Success = false,
                Message = "Error al cambiar contraseña",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
```

### 4. WebAPI - Agregar Endpoint (Finasist.Auth.WebAPI/Controllers/)

```csharp
[HttpPost("change-password")]
[Authorize] // Requiere autenticación
public async Task<ActionResult<BaseResponse<bool>>> ChangePassword([FromBody] ChangePasswordDto request)
{
    var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
    
    var command = new ChangePasswordCommand
    {
        UserId = userId,
        CurrentPassword = request.CurrentPassword,
        NewPassword = request.NewPassword
    };
    
    var result = await _mediator.Send(command);
    return result.Success ? Ok(result) : BadRequest(result);
}
```

---

## Ejemplo: Agregar Query "Obtener Usuarios Activos"

### 1. Core - Crear Query

```csharp
public class GetActiveUsersQuery : IRequest<BaseResponse<IEnumerable<UserDto>>>
{
}
```

### 2. Infrastructure - Agregar método al repositorio

```csharp
// En IUserRepository
Task<IEnumerable<User>> GetActiveUsersAsync();

// En DapperUserRepository
public async Task<IEnumerable<User>> GetActiveUsersAsync()
{
    using var connection = new SqlConnection(_connectionString);
    const string sql = @"
        SELECT Id, Username, Email, PasswordHash, Activo, FechaCreacion 
        FROM Users 
        WHERE Activo = 1
        ORDER BY FechaCreacion DESC";
    
    return await connection.QueryAsync<User>(sql);
}
```

### 3. Application - Crear Handler

```csharp
public class GetActiveUsersQueryHandler : IRequestHandler<GetActiveUsersQuery, BaseResponse<IEnumerable<UserDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetActiveUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BaseResponse<IEnumerable<UserDto>>> Handle(GetActiveUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var users = await _unitOfWork.Users.GetActiveUsersAsync();
            
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Status = UserStatus.Activo,
                FechaCreacion = u.FechaCreacion
            });

            return new BaseResponse<IEnumerable<UserDto>>
            {
                Success = true,
                Data = userDtos
            };
        }
        catch (Exception ex)
        {
            return new BaseResponse<IEnumerable<UserDto>>
            {
                Success = false,
                Message = "Error al obtener usuarios activos",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
```

### 4. WebAPI - Agregar Endpoint

```csharp
[HttpGet("active-users")]
[Authorize(Roles = "Admin")]
public async Task<ActionResult<BaseResponse<IEnumerable<UserDto>>>> GetActiveUsers()
{
    var query = new GetActiveUsersQuery();
    var result = await _mediator.Send(query);
    return Ok(result);
}
```

---

## Checklist para Nuevas Funcionalidades

### Commands (Escritura)
- [ ] Crear Command en Core/Commands
- [ ] Crear DTO en Core/DTOs (si es necesario)
- [ ] Crear CommandHandler en Application/Handlers
- [ ] Agregar método al repositorio si es necesario
- [ ] Agregar endpoint en Controller
- [ ] Probar con Swagger

### Queries (Lectura)
- [ ] Crear Query en Core/Queries
- [ ] Crear QueryHandler en Application/Handlers
- [ ] Agregar método optimizado con Dapper en repositorio
- [ ] Agregar endpoint en Controller
- [ ] Probar con Swagger

---

## Buenas Prácticas

1. **Commands**: Siempre retornan BaseResponse<T>
2. **Queries**: Solo lectura, no modifican datos
3. **Handlers**: Un handler por Command/Query
4. **Dapper**: Usar para queries complejas y optimizadas
5. **Validación**: Usar FluentValidation o DataAnnotations
6. **Logging**: Usar ILogger en handlers críticos
7. **Transacciones**: Usar UnitOfWork para operaciones múltiples
8. **Seguridad**: Usar [Authorize] en endpoints protegidos

---

## Ejemplo de Transacción con UnitOfWork

```csharp
public async Task<BaseResponse<bool>> Handle(CreateUserWithRolesCommand request, CancellationToken cancellationToken)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // Crear usuario
        var user = new User { /* ... */ };
        await _unitOfWork.Users.AddAsync(user);

        // Asignar roles
        foreach (var roleId in request.RoleIds)
        {
            // Lógica para asignar roles
        }

        await _unitOfWork.CommitTransactionAsync();

        return new BaseResponse<bool> { Success = true };
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return new BaseResponse<bool> 
        { 
            Success = false, 
            Errors = new List<string> { ex.Message } 
        };
    }
}
```
