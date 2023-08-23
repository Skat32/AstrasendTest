using System.Diagnostics;
using Astrasend.Infrastructure.Np.Extensions.Exceptions;
using Astrasend.Infrastructure.Np.Extensions.Result;
using Astrasend.Infrastructure.Np.Logging;
using MediatR;
using Serilog;
using Serilog.Core;

namespace Astrasend.Infrastructure.Np.MediatR;

/// <summary>
/// Logging behavior for mediatr
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<IRequest<TResponse>, TResponse> where TRequest : notnull
{
    private readonly ILogger _logger = Log.Logger.ForContext(Constants.SourceContextPropertyName, "Mediatr");
    
    /// <inheritdoc />
    public async Task<TResponse> Handle(IRequest<TResponse> request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;

        try
        {
            _logger
                .ForContextJson("Request", request)
                .Information($"Handling {requestName}");
        }
        catch (Exception ex)
        {
            _logger
                .ForContextJson("Exception", ex)
                .Information($"Failed {requestName} in {sw.Elapsed.TotalMilliseconds} ms");
        }

        TResponse response;
        try
        {
            response = await next();

            if (response is ResultBase { IsSuccess: false, WasErrorReported: false } result)
                result.WasErrorReported = true;
        }
        catch (CustomException ex)
        {
            _logger
                .ForContextJson("Exception", ex)
                .Information($"Failed {requestName}");

            throw;
        }
        catch (Exception ex)
        {
            _logger
                .ForContextJson("Exception", ex)
                .Information($"Failed {requestName}");

            throw;
        }
        finally
        {
            sw.Stop();
        }

        _logger
            .ForContextJson("Response", response!)
            .Information($"Handled {requestName} in {sw.Elapsed.TotalMilliseconds} ms");

        return response;
    }
}
