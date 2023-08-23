using Astrasend.Infrastructure.Np.Logging.Http.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Astrasend.Infrastructure.Np.Logging.Http
{
    /// <inheritdoc />
    internal class LoggingStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<RequestResponseLoggingMiddleware>();
                
                next(app);
            };
        }
    }
}