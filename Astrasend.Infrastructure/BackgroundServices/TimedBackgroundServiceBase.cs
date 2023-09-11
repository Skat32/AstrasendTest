using Cronos;
using Medallion.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTracing.Util;
using Serilog;

namespace Astrasend.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Базовый бэкграунд сервис
    /// </summary>
    public abstract class TimedBackgroundServiceBase : IHostedService, IDisposable
    {
        private Task? _currentTask;
        private Timer? _timer;
        private readonly string _serviceName;
        private readonly CancellationTokenSource _cts;
        private readonly IDistributedLockProvider _distributedLockProvider;

        /// <summary>
        /// Service Provider
        /// </summary>
        protected IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// ctor
        /// </summary>
        protected TimedBackgroundServiceBase(IServiceProvider serviceProvider, ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _serviceName = GetType().FullName!;
            _cts = new CancellationTokenSource();
            _distributedLockProvider = ServiceProvider.GetRequiredService<IDistributedLockProvider>();
        }

        /// <inheritdoc/>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Starting timed background service: {serviceType}", _serviceName);

            ConditionalSetTimer();

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Information("Stopping timed background service: {serviceType}", _serviceName);

            _timer?.Change(Timeout.Infinite, -1);

            if (_currentTask == null || _currentTask.IsCompleted)
            {
                return;
            }

            try
            {
                _cts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_currentTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        private async void HandleTimerCallbackAsync(object? state) //https://stackoverflow.com/a/38918443
        {
            using var span = GlobalTracer.Instance.BuildSpan(_serviceName).StartActive();
            Logger.Information("Performing scheduled job in timed background service: {serviceType}",
                _serviceName);

            try
            {
                await using var handle = TryAcquireLock(_cts.Token);
                
                if (handle != null)
                {
                    Logger.Information("Lock acquired: {serviceType}", _serviceName);

                    _currentTask = DoWork(_cts.Token);
                    await _currentTask;

                    Logger.Information("Scheduled job has finished in timed background service: {serviceType}",
                        _serviceName);
                }
                else
                {
                    Logger.Information("Unable to acquire lock: {serviceType}", _serviceName);
                }
            }
            catch (OperationCanceledException e)
            {
                Logger.Warning(e, "Operation was canceled in timed background service: {serviceType}",
                    _serviceName);
            }
            catch (Exception e)
            {
                Logger.Error(e,
                    "Caught unhandled exception in timed background service: {serviceType}",
                    _serviceName);
            }
            finally
            {
                _currentTask = null;
                ConditionalSetTimer();
            }
        }

        private IDistributedSynchronizationHandle? TryAcquireLock(CancellationToken cancellationToken)
        {
            var settings = GetSettings();
            for (var i = 0; i < settings.MaxConcurrentJobs; i++)
            {
                var handle = _distributedLockProvider.TryAcquireLock(_serviceName + i, default, cancellationToken);

                if (handle != null)
                    return handle;
            }

            return null;
        }

        /// <summary>
        /// Get current timer settings
        /// </summary>
        /// <returns></returns>
        protected abstract TimedBackgroundServiceSettingsBase GetSettings();

        /// <summary>
        /// Scheduled job
        /// </summary>
        /// <returns></returns>
        protected abstract Task DoWork(CancellationToken cancellationToken);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Dispose"/>
        protected virtual void Dispose(bool disposing)
        {
            _timer?.Dispose();
            _cts.Cancel();
        }

        /// <summary>
        /// Обновить таймер
        /// </summary>
        private void ConditionalSetTimer()
        {
            if (_cts.IsCancellationRequested)
                return;

            var settings = GetSettings();
            
            if (string.IsNullOrWhiteSpace(settings.Cron) || settings.IsDisabled) 
                return;
            
            var nextOccurrence = GetScheduledTime(settings.Cron);
            var dueTime = nextOccurrence - DateTime.UtcNow;
            if (dueTime.Ticks < 0)
                dueTime = TimeSpan.Zero;

            if (_timer == null)
                _timer = new Timer(HandleTimerCallbackAsync, null, dueTime, TimeSpan.Zero);
            else
                _timer.Change(dueTime, TimeSpan.Zero);

            Logger.Information("Запланирован вызов сервиса {serviceType} в " + nextOccurrence + " UTC", _serviceName);
        }

        /// <summary>
        /// Распарсить cron-выражение и вернуть ближайшую запланированную дату
        /// </summary>
        private static DateTime GetScheduledTime(string cron)
        {
            var format = CronFormat.Standard;
            if (cron.Split(' ').Length > 5)
                format = CronFormat.IncludeSeconds;

            CronExpression expr;
            try
            {
                expr = CronExpression.Parse(cron, format);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Неверный формат cron-выражения: {cron}", ex);
            }

            return expr.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")) 
                   ?? throw new InvalidOperationException($"Не удалось определить ближайшую дату: {cron}");
        }
    }
}
