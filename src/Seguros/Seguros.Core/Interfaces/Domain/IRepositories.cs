using Seguros.Domain.Entities;

namespace Seguros.Core.Interfaces.Domain
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente> GetByUserIdAsync(int id);
        Task<Cliente> GetByIdentificacionAsync(string numeroIdentificacion);
        Task<Cliente> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string numeroIdentificacion);
    }

    public interface IPolizaRepository : IRepository<Poliza>
    {
        Task<IEnumerable<Poliza>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<Poliza>> GetByTipoPolizaAsync(int tipoPoliza);
        Task<IEnumerable<Poliza>> GetByEstatusAsync(int estatus);
        Task<IEnumerable<Poliza>> GetPolizasVigentesAsync();
        Task<IEnumerable<Poliza>> GetPolizasVencidasAsync();
        Task DeleteAsyncByClienteIdAsync(int id);
    }

    public interface IUnitOfWork : IDisposable
    {
        IClienteRepository Clientes { get; }
        IPolizaRepository Polizas { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}