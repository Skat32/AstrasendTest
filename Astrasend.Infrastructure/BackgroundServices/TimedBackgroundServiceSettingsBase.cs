using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Timer settings for <see cref="TimedBackgroundServiceBase"/>
    /// </summary>
    public class TimedBackgroundServiceSettingsBase
    {
        /// <summary>
        /// Cron-выражение для запуска бэкграунд сервиса по расписанию
        /// </summary>
        public string? Cron { get; set; }

        /// <summary>
        /// Max number of jobs working simultaneously.
        /// <remarks>2 - 2 jobs allowed</remarks>
        /// <remarks>0 - jobs are disabed</remarks>
        /// <remarks>-1 - ignore</remarks>
        /// </summary>
        [Required]
        public int MaxConcurrentJobs { get; set; }

        /// <summary>
        /// Выключен ли бэкграунд сервис
        /// </summary>
        public bool IsDisabled { get; set; } = false;
    }
}
