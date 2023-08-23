using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Astrasend.Api.Configuration;

/// <summary>
/// Расширения для постойки приложения
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Применить миграцию бд для контекста ef
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TContext"></typeparam>
    public static void MigrationDbContext<TContext>(this IApplicationBuilder builder) where TContext : DbContext
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>().Database;
        db.Migrate();

        using var connection = (NpgsqlConnection)db.GetDbConnection();
        connection.Open();
        connection.ReloadTypes();
    }
}