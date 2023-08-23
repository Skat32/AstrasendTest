using System.Reflection;
using Astrasend.Application.ApiClients;
using Astrasend.Application.Commands.PaymentProcessing;
using Astrasend.Application.Consumers;
using Astrasend.DataLayer;
using Astrasend.DataLayer.Repositories;
using Astrasend.Infrastructure.Np.Extensions.DependencyInjection;
using Astrasend.Infrastructure.Np.Logging.Http;
using Astrasend.Infrastructure.Np.MediatR;
using Astrasend.Infrastructure.Np.Policy.ClientPolicy;
using Astrasend.Infrastructure.Np.Policy.ClientPolicy.Interfaces;
using Astrasend.Infrastructure.Np.Policy.Settings;
using Astrasend.Infrastructure.Np.PostgreSQL.Settings;
using Astrasend.Infrastructure.Np.RabbitMQ.Settings;
using Astrasend.Infrastructure.Settings;
using Medallion.Threading;
using Medallion.Threading.Postgres;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;

namespace Astrasend.Api.Configuration;

/// <summary>
/// Расширение для настройки сервисов
/// </summary>
public static class ServiceCollectionExtensions
{
  
    /// <summary>
    /// ConfigureHttpClients
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureHttpClients(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        var binSettings = serviceProvider.GetRequiredService<IOptions<ApiClientSettings>>().Value;
        var clientPolicy = serviceProvider.GetRequiredService<IClientPolicy>();
        
        services.AddHttpClient<IApiClient, ApiClient>(client =>
                client.WithBaseAddress(binSettings.Url))
            .AddTransientHttpErrorPolicy(x => clientPolicy.GetPolicy(x))
            .AddHttpMessageHandler<LoggingHandler>();

        services.AddHttpClient();
    }

    /// <summary>
    /// Конфигурация апи клиентов
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureApiClients(this IServiceCollection services)
    {
        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IClientPolicy, ClientPolicy>();
    }
    
    /// <summary>
    /// Конфигурирование классов настроек
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    public static void ConfigureSettings(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureSettings<DbSettings>(config, nameof(DbSettings));
        services.ConfigureSettings<RabbitMqSettings>(config, nameof(RabbitMqSettings));
        services.ConfigureSettings<OutRequestRetrySettings>(config, nameof(OutRequestRetrySettings));
        services.ConfigureSettings<ApiClientSettings>(config, nameof(ApiClientSettings));
        services.ConfigureSettings<InvoicePaymentConsumerSettings>(config, nameof(InvoicePaymentConsumerSettings));
    }

    /// <summary>
    /// Конфигурирование бд
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureDatabase(this IServiceCollection services)
    {
        services.AddDbContext<DataDbContext>((provider, options) =>
        {
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
            
            options.UseNpgsql(GetConnectionString(dbSettings),
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public"));
            
            options.ConfigureWarnings(warning => warning
                    .Log(RelationalEventId.CommandExecuting))
                .UseLoggerFactory(loggerFactory)
                .EnableSensitiveDataLogging();
        }, ServiceLifetime.Scoped, ServiceLifetime.Singleton);

        services.AddScoped<IOperationRepository, OperationRepository>();
    }

    /// <summary>
    /// ConfigureApplicationServices
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TimingsBehavior<,>));
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        
        services.AddSingleton<IDistributedLockProvider>(provider =>
        {
            var dbSettings = provider.GetRequiredService<IOptions<DbSettings>>().Value;
            var connectionString = GetConnectionString(dbSettings);
        
            return new PostgresDistributedSynchronizationProvider(connectionString);
        });


        var assemblies = new[]
        {
            // Explicit assemblies enumeration: AppDomain.CurrentDomain.GetAssemblies()
            // doesn't show all assemblies as loaded in memory during startup
            Assembly.GetExecutingAssembly(),
            Assembly.GetAssembly(typeof(PaymentProcessingCommand)), // Application
        };

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies!));
    }

    /// <summary>
    /// Configure RabbitMq Services
    /// </summary>
    public static void ConfigureRabbitMqServices(this IServiceCollection services)
    {
        services.AddHostedService<InvoicePaymentConsumer>();

        services.AddSingleton(serviceProvider =>
        {
            var rabbitSettings = serviceProvider.GetRequiredService<IOptions<RabbitMqSettings>>().Value;
            var uri = new Uri($"{rabbitSettings.Protocol}://{rabbitSettings.UserName}:{rabbitSettings.Password}" +
                              $"@{rabbitSettings.Host}:{rabbitSettings.Port}/{rabbitSettings.VirtualHost}");
            return new ConnectionFactory
            {
                Uri = uri,
                DispatchConsumersAsync = true
            };
        });
    }

    private static string GetConnectionString(DbSettings settings)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = settings.Host,
            Port = int.Parse(settings.Port),
            Database = settings.DbName,
            Username = settings.User,
            Password = settings.Password,
            NoResetOnClose = true,
            SearchPath = settings.Schema,
            MaxPoolSize = settings.MaxPoolSize,
            CommandTimeout = Math.Max(settings.CommandTimeout, 30)
        };

        return connectionStringBuilder.ConnectionString;
    }
    
    /// <summary>
    /// Добавление базового адреса сервиса
    /// </summary>
    private static HttpClient WithBaseAddress(this HttpClient httpClient, string url)
    {
        if (httpClient == null) throw new ArgumentNullException(nameof(httpClient));
        if (string.IsNullOrEmpty(url)) throw new ArgumentException(null, nameof(url));

        httpClient.BaseAddress = new Uri(url);

        return httpClient;
    }
}