using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Auth.Core.Interfaces.Domain;
using Auth.Domain.Entities;

namespace Auth.Infrastructure.Data
{
    public class DapperUserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public DapperUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, Activo, FechaCreacion 
                FROM Users 
                WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, Activo, FechaCreacion 
                FROM Users 
                WHERE Username = @Username";
            
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, Activo, FechaCreacion 
                FROM Users 
                WHERE Email = @Email";
            
            return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
        }

        public async Task<bool> ExistsAsync(string username, string email)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT COUNT(1) 
                FROM Users 
                WHERE Username = @Username OR Email = @Email";
            
            var count = await connection.QueryFirstAsync<int>(sql, new { Username = username, Email = email });
            return count > 0;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Username, Email, PasswordHash, Activo, FechaCreacion 
                FROM Users 
                ORDER BY FechaCreacion DESC";
            
            return await connection.QueryAsync<User>(sql);
        }

        public async Task<User> AddAsync(User entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO Users (Username, Email, PasswordHash, Activo, FechaCreacion)
                OUTPUT INSERTED.Id
                VALUES (@Username, @Email, @PasswordHash, @Activo, @FechaCreacion)";
            
            var id = await connection.QuerySingleAsync<int>(sql, entity);
            entity.Id = id;
            return entity;
        }

        public async Task UpdateAsync(User entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE Users 
                SET Username = @Username, 
                    Email = @Email, 
                    PasswordHash = @PasswordHash, 
                    Activo = @Activo
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Users WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public class DapperRoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public DapperRoleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Nombre, Descripcion, Activo, FechaCreacion 
                FROM Roles 
                WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
        }

        public async Task<Role> GetByNameAsync(string name)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Nombre, Descripcion, Activo, FechaCreacion 
                FROM Roles 
                WHERE Nombre = @Nombre";
            
            return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Nombre = name });
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT r.Id, r.Nombre, r.Descripcion, r.Activo, r.FechaCreacion
                FROM Roles r
                INNER JOIN UserRoles ur ON r.Id = ur.RoleId
                WHERE ur.UserId = @UserId";
            
            return await connection.QueryAsync<Role>(sql, new { UserId = userId });
        }

        public async Task AddUserRoleAsync(int userId, int roleId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO UserRoles (UserId, RoleId)
                VALUES (@UserId, @RoleId)";
            
            await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, Nombre, Descripcion, Activo, FechaCreacion 
                FROM Roles 
                ORDER BY Nombre";
            
            return await connection.QueryAsync<Role>(sql);
        }

        public async Task<Role> AddAsync(Role entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO Roles (Nombre, Descripcion, Activo, FechaCreacion)
                OUTPUT INSERTED.Id
                VALUES (@Nombre, @Descripcion, @Activo, @FechaCreacion)";
            
            var id = await connection.QuerySingleAsync<int>(sql, entity);
            entity.Id = id;
            return entity;
        }

        public async Task UpdateAsync(Role entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE Roles 
                SET Nombre = @Nombre, 
                    Descripcion = @Descripcion, 
                    Activo = @Activo
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Roles WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public class DapperRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public DapperRefreshTokenRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<RefreshToken> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, UserId, Token, FechaExp, FechaCreacion, Revocado 
                FROM RefreshTokens 
                WHERE Id = @Id";
            
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Id = id });
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, UserId, Token, FechaExp, FechaCreacion, Revocado 
                FROM RefreshTokens 
                WHERE Token = @Token";
            
            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, UserId, Token, FechaExp, FechaCreacion, Revocado 
                FROM RefreshTokens 
                WHERE UserId = @UserId 
                ORDER BY FechaCreacion DESC";
            
            return await connection.QueryAsync<RefreshToken>(sql, new { UserId = userId });
        }

        public async Task RevokeAsync(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE RefreshTokens 
                SET Revocado = 1 
                WHERE Token = @Token";
            
            await connection.ExecuteAsync(sql, new { Token = token });
        }

        public async Task DeleteExpiredTokensAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM RefreshTokens WHERE FechaExp < GETUTCDATE()";
            await connection.ExecuteAsync(sql);
        }

        public async Task<IEnumerable<RefreshToken>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT Id, UserId, Token, FechaExp, FechaCreacion, Revocado 
                FROM RefreshTokens 
                ORDER BY FechaCreacion DESC";
            
            return await connection.QueryAsync<RefreshToken>(sql);
        }

        public async Task<RefreshToken> AddAsync(RefreshToken entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO RefreshTokens (UserId, Token, FechaExp, FechaCreacion, Revocado)
                OUTPUT INSERTED.Id
                VALUES (@UserId, @Token, @FechaExp, @FechaCreacion, @Revocado)";
            
            var id = await connection.QuerySingleAsync<int>(sql, entity);
            entity.Id = id;
            return entity;
        }

        public async Task UpdateAsync(RefreshToken entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE RefreshTokens 
                SET UserId = @UserId, 
                    Token = @Token, 
                    FechaExp = @FechaExp, 
                    Revocado = @Revocado
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM RefreshTokens WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        public IUserRepository Users { get; }
        public IRoleRepository Roles { get; }
        public IRefreshTokenRepository RefreshTokens { get; }

        public DapperUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;
            Users = new DapperUserRepository(connectionString);
            Roles = new DapperRoleRepository(connectionString);
            RefreshTokens = new DapperRefreshTokenRepository(connectionString);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await Task.FromResult(0);
        }

        public async Task BeginTransactionAsync()
        {
            _connection = new SqlConnection(_connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                _transaction?.Commit();
            }
            catch
            {
                _transaction?.Rollback();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
            await Task.CompletedTask;
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                _transaction?.Rollback();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }
            await Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}