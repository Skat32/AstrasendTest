using System.Diagnostics;
using MediatR;

namespace Astrasend.Infrastructure.Np.MediatR;

/// <summary>
/// Timings behavior for mediatr
/// </summary>
public class TimingsBehavior<TRequest, TResponse> : IPipelineBehavior<IRequest<TResponse>, TResponse> where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(IRequest<TResponse> request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            return await next();
        }
        finally
        {
            sw.Stop();
        }
    }
}
