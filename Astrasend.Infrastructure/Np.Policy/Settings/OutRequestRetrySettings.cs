using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.Np.Policy.Settings;

/// <summary>
/// Настройки для Polly
/// </summary>
public class OutRequestRetrySettings
{
    /// <summary>
    /// Сколько ожидаем запрос перед очередной попыткой
    /// </summary>
    [Required]
    public int TimeoutInSeconds { get; set; }

    /// <summary>
    /// Количество попыток
    /// </summary>
    [Required]
    public int RetryCount { get; set; }
}
