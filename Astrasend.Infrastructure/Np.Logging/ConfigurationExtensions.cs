using System.Text.RegularExpressions;
using Astrasend.Infrastructure.Np.Logging.Enrichers;
using Astrasend.Infrastructure.Np.Logging.Formatters;
using Astrasend.Infrastructure.Np.Logging.Http;
using Astrasend.Infrastructure.Np.Logging.Http.Models;
using Astrasend.Infrastructure.Np.Logging.LogLevelSwitcher;
using Astrasend.Infrastructure.Np.Logging.Models;
using Destructurama;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Enrichers.Sensitive;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace Astrasend.Infrastructure.Np.Logging
{
   /// <summary>
    /// Useful extensions methods for serilog logger
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Using serilog with some defaults
        /// </summary>
        public static IWebHostBuilder UseLogging(
            this IWebHostBuilder builder,
            string applicationName,
            Action<WebHostBuilderContext, LoggerConfiguration> configureLogger,
            LogEventLevel minLogLevel = LogEventLevel.Information,
            IConfiguration? configuration = null,
            params IMaskingOperator[] maskingOperators)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (configureLogger == null) throw new ArgumentNullException(nameof(configureLogger));

            var logSwitch = new LoggingLevelSwitch(minLogLevel);
            var loggingSwitcher = new LoggingLevelSwitcher(logSwitch);
            
            var fullConfigureLogger = BasicConfigureLogger + configureLogger;

            builder.UseSerilog(fullConfigureLogger);

            builder.ConfigureServices((context, services) =>
            {
                services.AddLogSettings(context.Configuration);
                services.AddSingleton(Log.Logger);
                services.AddTransient<LoggingHandler>();
                services.AddSingleton<IStartupFilter, LoggingStartupFilter>();
                services.AddSingleton<ILoggingLevelSwitcher>(loggingSwitcher);
            });

            // Отключение стандартных сообщений при старте и остановке приложения. Они не структурированы и не могут быть корректно спарсены.
            builder.SuppressStatusMessages(true);

            return builder;
            
            void BasicConfigureLogger(WebHostBuilderContext ctx, LoggerConfiguration config)
            {
                var settings = configuration is null
                    ? GetElasticConfiguration()
                    : GetElasticConfiguration(configuration);

                // https://user:password@stack-server:port
                var elasticUrl = new Uri($"http://{settings.Login}:{settings.Password}@{settings.Uri}").ToString();

                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                var isLoglevel = Enum.TryParse<LogEventLevel>(ctx.Configuration["DEFAULT_LOGLEVEL"], true, out var result);

                var level = isLoglevel ? result : LogEventLevel.Information;
                config.MinimumLevel.Is(level)
                    .IgnoreTechnicalLogs()
                    .Enrich.With<OpenTracingLogsEnricher>()
                    .Enrich.WithSensitiveDataMasking(MaskingMode.Globally, maskingOperators)
                    .WriteToDefault()
                    .WriteTo.Elasticsearch(ConfigureElasticSink(elasticUrl, environment, applicationName))
                    .MinimumLevel.ControlledBy(logSwitch)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", applicationName)
                    .Enrich.WithMachineName();
            }
        }

        /// <summary>
        /// Common "Write.To" implementation
        /// </summary>
        /// <remarks>
        /// in Development env - write to console in friendly format, write to file
        /// in non Development env - write to console
        /// </remarks>
        private static LoggerConfiguration WriteToDefault(
            this LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}");

            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "Log.json");
            loggerConfiguration.WriteTo.File(new AddTypePostfixElasticSearchJsonFormatter(), logPath,
                rollingInterval: RollingInterval.Day);

            return loggerConfiguration;
        }

        /// <summary>
        /// Ignore log by log event property (works for first level property only)
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <param name="key"></param>
        /// <param name="regexp"></param>
        /// <returns></returns>
        private static LoggerConfiguration IgnoreLog(
            this LoggerConfiguration loggerConfiguration,
            string key, Regex regexp)
        {
            loggerConfiguration.Filter.ByExcluding(e =>
            {
                var hasKey = e.Properties.ContainsKey(key);
                if (!hasKey)
                    return false;

                var path = e.Properties[key]?.ToString();

                if (path == null)
                    return false;

                var matched = regexp.IsMatch(path);

                return matched;
            });

            return loggerConfiguration;
        }

        /// <summary>
        /// Ignores http logs with RequestPath /health /ready /cap /swagger /metrics-text
        /// </summary>
        /// <param name="loggerConfiguration"></param>
        /// <returns></returns>
        private static LoggerConfiguration IgnoreTechnicalLogs(this LoggerConfiguration loggerConfiguration)
        {
            return loggerConfiguration
                .IgnoreLog("HttpUri", new Regex(@".*\/cap(\/|\s|"")"))
                .IgnoreLog("HttpUri", new Regex(@".*\/health(\/|\s|"")"))
                .IgnoreLog("HttpUri", new Regex(@".*\/ready(\/|\s|"")"))
                .IgnoreLog("HttpUri", new Regex(@".*\/swagger(\/|\s|"")"))
                .IgnoreLog("HttpUri", new Regex(@".*\/metrics(\/|\s|"")"))
                .IgnoreLog("HttpUri", new Regex(@".*\/metrics-text(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/cap(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/health(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/ready(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/swagger(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/metrics(\/|\s|"")"))
                .IgnoreLog("RequestPath", new Regex(@".*\/metrics-text(\/|\s|"")"));
        }

        /// <summary>
        /// Exclude fields by pathes
        /// </summary>
        /// <param name="config">Logger configuration</param>
        /// <param name="fieldPaths">Field pathes</param>
        /// <returns></returns>
        public static LoggerConfiguration ExcludeFields(this LoggerConfiguration config, params string[] fieldPaths)
        {
            config.Enrich.With(new ExcludeFieldEnricher(fieldPaths));
            return config;
        }

        /// <summary>
        /// Exclude fields by pathes
        /// </summary>
        /// <param name="config">Logger configuration</param>
        /// <param name="fieldPaths">Field pathes (Field's separator in path: dot)</param>
        /// <returns></returns>
        public static LoggerConfiguration ExcludeFields(this LoggerConfiguration config, IEnumerable<string> fieldPaths)
        {
            return config.ExcludeFields(fieldPaths.ToArray());
        }

        /// <summary>
        /// Mask fields by pathes
        /// </summary>
        /// <param name="config">Logger configuration</param>
        /// <param name="fieldPaths">Field pathes</param>
        /// <returns></returns>
        public static LoggerConfiguration MaskFields(this LoggerConfiguration config, params string[] fieldPaths)
        {
            config.Enrich.With(new MaskFieldEnricher(fieldPaths));
            return config;
        }

        /// <summary>
        /// Mask fields by pathes
        /// </summary>
        /// <param name="config">Logger configuration</param>
        /// <param name="fieldPaths">Field pathes (Field's separator in path: dot)</param>
        /// <returns></returns>
        public static LoggerConfiguration MaskFields(this LoggerConfiguration config, IEnumerable<string> fieldPaths)
        {
            return config.MaskFields(fieldPaths.ToArray());
        }

        /// <summary>
        /// Add destructure serialized Json fields
        /// </summary>
        /// <param name="config">Logger configuration</param>
        /// <returns></returns>
        public static LoggerConfiguration DestructureJson(this LoggerConfiguration config)
        {
            config.Destructure.JsonNetTypes();
            return config;
        }

        private static void AddLogSettings(this IServiceCollection services, IConfiguration config)
        {
            var settingsSection = config.GetSection(nameof(LogSettings));

            services.AddOptions<LogSettings>()
                .Bind(settingsSection)
                .ValidateDataAnnotations();

            services.PostConfigure<LogSettings>(o => o.Init());
        }
        
        private static ElasticsearchSinkOptions ConfigureElasticSink(
            string elasticUrl,
            string? environment,
            string applicationName)
        {
            return new ElasticsearchSinkOptions(new Uri(elasticUrl))
            {
                AutoRegisterTemplate = true,
                TypeName = null,
                BatchAction = ElasticOpType.Create,
                IndexFormat = $"{applicationName.ToLower().Replace(".", "-")}" +
                              $"-{environment?.ToLower().Replace(".", "-")}"
            };
        }

        /// <summary>
        /// Получение настроек для ELK из конфигурации приложения
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">не был найден хотя бы один параметр</exception>
        private static ElasticConfiguration GetElasticConfiguration(IConfiguration configuration)
        {
            const string uriName = "ElasticConfiguration:Uri";
            const string loginName = "ElasticConfiguration:Login";
            const string passName = "ElasticConfiguration:Password";
            
            var uri = configuration.GetSection(uriName)?.Value;
            var login = configuration.GetSection(loginName)?.Value;
            var pass = configuration.GetSection(passName)?.Value;
            
            var elasticConfiguration = new ElasticConfiguration
            {
                Uri = uri ?? throw new ArgumentNullException(uriName),
                Login = login ?? throw new ArgumentNullException(loginName),
                Password = pass ?? throw new ArgumentNullException(passName)
            };
            return elasticConfiguration;
        }

        /// <summary>
        /// Получение настроек для ELK из env
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">не был найден хотя бы один параметр</exception>
        private static ElasticConfiguration GetElasticConfiguration()
        {
            const string uri = "ElasticConfiguration__Uri";
            const string login = "ElasticConfiguration__Login";
            const string password = "ElasticConfiguration__Password";

            return new ElasticConfiguration
            {
                Uri = Environment.GetEnvironmentVariable(uri) ?? throw new ArgumentNullException(uri),
                Login = Environment.GetEnvironmentVariable(login) ?? throw new ArgumentNullException(login),
                Password = Environment.GetEnvironmentVariable(password) ?? throw new ArgumentNullException(password)
            };
        }
    }
}
