using Microsoft.EntityFrameworkCore;

namespace Astrasend.Infrastructure.Np.PostgreSQL.Repository.Interfaces;

/// <summary>
/// Базовый класс управления репозиториями
/// </summary>
public interface IBaseRepository
{
    /// <summary><see cref="DbContext.SaveChangesAsync(System.Threading.CancellationToken)"/></summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary><see cref="DbContext.AddRangeAsync(object[])"/></summary>
    Task AddRangeAsync<T>(ICollection<T> collection, CancellationToken cancellationToken = default) where T : class;
        
    /// <summary><see cref="DbContext.UpdateRange(object[])"/></summary>
    void UpdateRange<T>(ICollection<T> collection) where T : class;
        
    /// <summary><see cref="DbContext.AddAsync{TEntity}"/></summary>
    ValueTask<T> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
}