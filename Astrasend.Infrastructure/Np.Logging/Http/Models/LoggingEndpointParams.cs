using Astrasend.Infrastructure.Np.Logging.Converters;
using Newtonsoft.Json;
using Serilog.Events;

namespace Astrasend.Infrastructure.Np.Logging.Http.Models
{
    /// <summary>
    /// Logging endpoint request params
    /// </summary>
    public class LoggingEndpointParams
    {
        /// <summary>
        /// Min loglevel for whole application
        /// </summary>
        [JsonConverter(typeof(CustomStringEnumConverter))]
        public LogEventLevel Loglevel { get; set; }
    }
}