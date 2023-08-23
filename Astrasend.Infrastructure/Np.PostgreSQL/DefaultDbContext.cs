using Astrasend.Infrastructure.Np.PostgreSQL.Extensions;
using Astrasend.Models.Entities.Base;
using Astrasend.Models.Entities.Base.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using static System.Linq.Expressions.Expression;

namespace Astrasend.Infrastructure.Np.PostgreSQL;

/// <inheritdoc />
public abstract class DefaultDbContext<T> : DbContext where T : DbContext
{
    /// <inheritdoc />
    public DefaultDbContext() { }

    /// <inheritdoc />
    public DefaultDbContext(DbContextOptions<T> options) : base(options) { }
    
    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new())
    {
        UpdateDate(ChangeTracker);

        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        UpdateDate(ChangeTracker);

        return base.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateDate(ChangeTracker);

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override int SaveChanges()
    {
        UpdateDate(ChangeTracker);

        return base.SaveChanges();
    }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEntity(modelBuilder);
        AutoChangeTableAndColumnName(modelBuilder);
        ApplyConfiguration(modelBuilder);
        SetQueryFilter(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }
    
    /// <summary>
    /// Устанавливаем фильтр на запросы к бд, в данном случае не будут подбираться данный с флагом IsDeleted.<br/>
    /// Есть возможность переопределения метод
    /// </summary>
    protected virtual void SetQueryFilter(ModelBuilder modelBuilder)
    {
        //Для классов наследников от BaseEntity только
        foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(x => x.ClrType.BaseType == typeof(BaseEntity)))
        {
            var parameter = Parameter(entity.ClrType, "x");
            
            // x=>x.IsDeleted == false
            var deletedCheck = Lambda(
                Equal(Property(parameter, nameof(ISoftDeletableEntity.IsDeleted)), Constant(false)), 
                parameter);

            modelBuilder.Entity(entity.ClrType).HasQueryFilter(deletedCheck);
        }
    }

    /// <summary>
    /// Используется для дополнительной конфигурации
    /// </summary>
    protected virtual void ApplyConfiguration(ModelBuilder modelBuilder) { }
    
    /// <summary>
    /// Используется для дополнительной конфигурации сущностей
    /// </summary>
    protected virtual void ConfigureEntity(ModelBuilder modelBuilder) { }

    /// <summary>
    /// Класс автоматической установки дат создания и обновления сущности + реализация soft deleted 
    /// </summary>
    private static void UpdateDate(ChangeTracker changeTracker)
    {
        var dateTimeNow = DateTime.UtcNow;

        foreach (var entityEntry in changeTracker.Entries())
        {
            if (entityEntry is { Entity: IHardDeletableEntity, State: EntityState.Deleted })
                continue;

            if (entityEntry is { Entity: ISoftDeletableEntity entityDelete, State: EntityState.Deleted })
            {
                entityDelete.Delete();
                entityEntry.State = EntityState.Modified;
            }

            if (entityEntry is { Entity: IUpdatable entityUpdate, State: EntityState.Modified or EntityState.Added })
                entityUpdate.SetDate(dateTimeNow);

            if (entityEntry is { Entity: ICreatable entityCreate, State: EntityState.Added } &&
                entityCreate.CreatedAt == default)
                entityCreate.SetDate(dateTimeNow);
        }
    }

    /// <summary>
    /// Автоматическое преобразование имен таблиц и столбцов в БД
    /// </summary>
    private static void AutoChangeTableAndColumnName(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Replace table names
            entity.SetTableName(entity.GetTableName()!.ToSnakeCase());

            // Replace column names
            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.Name.ToSnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()!.ToSnakeCase());

            foreach (var key in entity.GetForeignKeys())
                key.PrincipalKey.SetName(key.PrincipalKey.GetName()!.ToSnakeCase());

            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName()!.ToSnakeCase());
        }
    }
}