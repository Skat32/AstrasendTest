using Serilog.Core;
using Serilog.Events;

namespace Astrasend.Infrastructure.Np.Logging.LogLevelSwitcher
{
    /// <inheritdoc />
    internal class LoggingLevelSwitcher : ILoggingLevelSwitcher
    {
        private readonly LoggingLevelSwitch _globalSwitch;

        /// <inheritdoc />
        /// <remarks>
        /// Default value = LogEventLevel.Information
        /// </remarks>
        public LogEventLevel GlobalLogLevel
        {
            get => _globalSwitch.MinimumLevel;
            set => _globalSwitch.MinimumLevel = value;
        }
        
        internal LoggingLevelSwitcher(LoggingLevelSwitch loggingSwitch)
        {
            _globalSwitch = loggingSwitch;
        }
    }
}