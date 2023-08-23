using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.Np.RabbitMQ.Settings;

/// <summary>
/// Базовый класс настроек для потребителя и отправителя
/// </summary>
public class BaseRabbitQueueSettings
{
    /// <summary>
    /// Exchange
    /// </summary>
    [Required]
    public string Exchange { get; set; }

    /// <summary>
    /// Имя очереди
    /// </summary>
    [Required]
    public string Queue { get; set; }
}