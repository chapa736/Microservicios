using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Seguros.Core.Interfaces.Domain;
using Seguros.Domain.Entities;

namespace Seguros.Infrastructure.Data
{
    public class DapperClienteRepository : IClienteRepository
    {
        private readonly string _connectionString;

        public DapperClienteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Cliente> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Clientes WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
        }
        public async Task<Cliente> GetByUserIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Clientes WHERE UserId = @Id";
            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
        }

        public async Task<Cliente> GetByIdentificacionAsync(string numeroIdentificacion)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Clientes WHERE NumeroIdentificacion = @NumeroIdentificacion";
            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { NumeroIdentificacion = numeroIdentificacion });
        }

        public async Task<Cliente> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Clientes WHERE Email = @Email";
            return await connection.QueryFirstOrDefaultAsync<Cliente>(sql, new { Email = email });
        }

        public async Task<bool> ExistsAsync(string numeroIdentificacion)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT COUNT(1) FROM Clientes WHERE NumeroIdentificacion = @NumeroIdentificacion";
            var count = await connection.QueryFirstAsync<int>(sql, new { NumeroIdentificacion = numeroIdentificacion });
            return count > 0;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Clientes ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Cliente>(sql);
        }

        public async Task<Cliente> AddAsync(Cliente entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO Clientes (NumeroIdentificacion, Nombre, ApPaterno, ApMaterno, Telefono, Email, Direccion, FechaCreacion, UserId)
                OUTPUT INSERTED.Id
                VALUES (@NumeroIdentificacion, @Nombre, @ApPaterno, @ApMaterno, @Telefono, @Email, @Direccion, @FechaCreacion, @UserId)";
            
            var id = await connection.QuerySingleAsync<int>(sql, entity);
            entity.Id = id;
            return entity;
        }

        public async Task UpdateAsync(Cliente entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE Clientes 
                SET Nombre = @Nombre, 
                    ApPaterno = @ApPaterno, 
                    ApMaterno = @ApMaterno, 
                    Telefono = @Telefono, 
                    Email = @Email, 
                    Direccion = @Direccion,
                    FechaActualizacion = @FechaActualizacion
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Clientes WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public class DapperPolizaRepository : IPolizaRepository
    {
        private readonly string _connectionString;

        public DapperPolizaRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Poliza> GetByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Polizas WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Poliza>(sql, new { Id = id });
        }

        public async Task<IEnumerable<Poliza>> GetByClienteIdAsync(int clienteId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Polizas WHERE IdCliente = @ClienteId ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Poliza>(sql, new { ClienteId = clienteId });
        }

        public async Task<IEnumerable<Poliza>> GetByTipoPolizaAsync(int tipoPoliza)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Polizas WHERE TipoPoliza = @TipoPoliza ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Poliza>(sql, new { TipoPoliza = tipoPoliza });
        }

        public async Task<IEnumerable<Poliza>> GetByEstatusAsync(int estatus)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Polizas WHERE Estatus = @Estatus ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Poliza>(sql, new { Estatus = estatus });
        }

        public async Task<IEnumerable<Poliza>> GetPolizasVigentesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Polizas 
                WHERE Estatus = 1 
                ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Poliza>(sql);
        }

        public async Task<IEnumerable<Poliza>> GetPolizasVencidasAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                SELECT * FROM Polizas 
                WHERE FechaFin < GETDATE() 
                ORDER BY FechaFin DESC";
            return await connection.QueryAsync<Poliza>(sql);
        }

        public async Task<IEnumerable<Poliza>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "SELECT * FROM Polizas ORDER BY FechaCreacion DESC";
            return await connection.QueryAsync<Poliza>(sql);
        }

        public async Task<Poliza> AddAsync(Poliza entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                INSERT INTO Polizas (IdCliente, TipoPoliza, FechaInicio, FechaFin, Monto, Estatus, FechaCreacion)
                OUTPUT INSERTED.Id
                VALUES (@IdCliente, @TipoPoliza, @FechaInicio, @FechaFin, @Monto, @Estatus, @FechaCreacion)";
            
            var id = await connection.QuerySingleAsync<int>(sql, entity);
            entity.Id = id;
            return entity;
        }

        public async Task UpdateAsync(Poliza entity)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = @"
                UPDATE Polizas 
                SET FechaFin = @FechaFin, 
                    Monto = @Monto, 
                    Estatus = @Estatus
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, entity);
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Polizas WHERE Id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task DeleteAsyncByClienteIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string sql = "DELETE FROM Polizas WHERE IdCliente = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }

    public class DapperUnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;

        public IClienteRepository Clientes { get; }
        public IPolizaRepository Polizas { get; }

        public DapperUnitOfWork(string connectionString)
        {
            _connectionString = connectionString;
            Clientes = new DapperClienteRepository(connectionString);
            Polizas = new DapperPolizaRepository(connectionString);
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
