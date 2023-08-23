using Astrasend.Infrastructure.Np.PostgreSQL.Repository.Interfaces;

namespace Astrasend.Infrastructure.Np.PostgreSQL.Repository
{
    
    /// <inheritdoc />
    public abstract class BaseRepository<TContext> : IBaseRepository where TContext : DefaultDbContext<TContext>
    {
        /// <summary>
        /// DbContext
        /// </summary>
        protected readonly TContext Context;

        /// <summary>ctor</summary>
        protected BaseRepository(TContext context)
        {
            Context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            await Context.SaveChangesAsync(cancellationToken);

        /// <inheritdoc />
        public async Task AddRangeAsync<T>(ICollection<T> collection, CancellationToken cancellationToken = default)
            where T : class => await Context.Set<T>().AddRangeAsync(collection, cancellationToken);

        /// <inheritdoc />
        public void UpdateRange<T>(ICollection<T> collection) where T : class => 
            Context.Set<T>().UpdateRange(collection);

        /// <inheritdoc />
        public async ValueTask<T> AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            var result = await Context.Set<T>().AddAsync(entity, cancellationToken);
            
            return result.Entity;
        }
    }
}
