using Auth.Domain.Entities;

namespace Auth.Core.Interfaces.Domain
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }

    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<bool> ExistsAsync(string username, string email);
    }

    public interface IRoleRepository : IRepository<Role>
    {
        Task<Role> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetUserRolesAsync(int userId);
        Task AddUserRoleAsync(int userId, int roleId);
    }

    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
        Task RevokeAsync(string token);
        Task DeleteExpiredTokensAsync();
    }

    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}