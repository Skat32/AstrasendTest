using System.Net;
using Astrasend.Infrastructure.Np.Extensions.Exceptions;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Astrasend.Infrastructure.Np.Extensions.DependencyInjection;

/// <summary>
/// Options Extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Конфигурирование ProblemDetails
    /// </summary>
    /// <remarks>Регистрировать ПОСЛЕ MVC-middleware (из-за настройки ApiBehaviorOptions)</remarks>
    public static void ConfigureProblemDetails(
        this IServiceCollection services,
        Action<ProblemDetailsOptions>? configure = null,
        bool includeExceptionDetails = false)
    {
        services.AddProblemDetails(c =>
        {
            c.IncludeExceptionDetails = (_, _) => includeExceptionDetails;

            c.Map<HttpStatusCodeException>(ex =>
            {
                if (ex.HttpStatusCode == (int) HttpStatusCode.ProxyAuthenticationRequired)
                    return new ProblemDetails
                    {
                        Type = "https://httpstatuses.com/500",
                        Title = "Internal Server Error",
                        Status = 500,
                        Detail = "Proxy authentication problem while requesting dependency"
                    };

                return new StatusCodeProblemDetails(ex.HttpStatusCode)
                {
                    Detail = ex.Message
                };
            });

            configure?.Invoke(c);

            c.Map<Exception>(ex => new StatusCodeProblemDetails(500));
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            // rewrite default ProblemDetails data to remove TraceId field from response
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Type = "ValidationError"
                };
                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = {"application/problem+json", "application/problem+xml"}
                };
            };
        });
    }

    /// <summary>
    /// Configure options from <see cref="IConfiguration"/>
    /// </summary>
    public static void ConfigureSettings<T>(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName)
        where T : class, new()
    {
        services.ConfigureSettings<T>(config, sectionName, _ => { });
    }

    /// <summary>
    /// Configure options from <see cref="IConfiguration"/>
    /// </summary>
    public static void ConfigureSettings<T>(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName,
        Action<T> configure)
        where T : class, new()
    {
        var section = config.GetSection(sectionName);
        var settings = section.Get<T>();
            
        if (settings == null)
            throw new InvalidOperationException($"{sectionName} section not found in configuration file");

        services.AddOptions<T>()
            .Bind(section)
            .Configure(configure)
            .ValidateDataAnnotations();

        var serviceProvider = services.BuildServiceProvider();
        _ = serviceProvider.GetRequiredService<IOptions<T>>().Value; //validate inside
    }
}