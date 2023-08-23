using Astrasend.Infrastructure.Np.PostgreSQL.Repository;

namespace Astrasend.DataLayer.Repositories;

/// <inheritdoc cref="Astrasend.DataLayer.Repositories.IOperationRepository" />
public class OperationRepository : BaseRepository<DataDbContext>, IOperationRepository
{
    /// ctor
    public OperationRepository(DataDbContext context) : base(context){}
}