using Astrasend.Infrastructure.BackgroundServices;
using Astrasend.Infrastructure.Settings;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Astrasend.Application.BackgroundServices;

/// <summary>
/// Бэкграунд сервси для удаления старых токенов
/// </summary>
public class TestBackgroundService : TimedBackgroundServiceBase
{
    private readonly TestSettings _settings;

    /// ctor
    public TestBackgroundService(IServiceProvider serviceProvider, ILogger logger,
        IOptions<TestSettings> settings) 
        : base(serviceProvider, logger)
    {
        _settings = settings.Value;
    }

    /// <inheritdoc />
    protected override TimedBackgroundServiceSettingsBase GetSettings()
    {
        return _settings;
    }

    /// <inheritdoc />
    protected override async Task DoWork(CancellationToken cancellationToken)
    {
        using var scope = ServiceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        // await mediator.Send(new TestCommand(), cancellationToken);
    }
}