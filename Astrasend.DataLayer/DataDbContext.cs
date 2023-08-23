using Astrasend.Infrastructure.Np.PostgreSQL;
using Astrasend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Astrasend.DataLayer;

/// <inheritdoc />
public class DataDbContext : DefaultDbContext<DataDbContext>
{
    /// <summary>
    /// Операции
    /// </summary>
    public DbSet<Operation> Operations => Set<Operation>();

    /// ctor
    public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }
}