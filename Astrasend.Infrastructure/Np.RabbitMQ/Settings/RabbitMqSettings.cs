using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.Np.RabbitMQ.Settings;

/// <summary>
/// Настройки для RabbitMq
/// </summary>
public class RabbitMqSettings
{
    /// <summary>
    /// Адрес
    /// </summary>
    [Required]
    public string Host { get; set; }
    
    /// <summary>
    /// Порт
    /// </summary>
    [Required]
    public string Port { get; set; }

    /// <summary>
    /// Протокол, по которому подключаться
    /// </summary>
    [Required]
    public string Protocol { get; set; } = "amqp";
    
    /// <summary>
    /// Виртуальный хост, в котором храняться очереди
    /// </summary>
    [Required]
    public string VirtualHost { get; set; }
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    public string UserName { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    public string Password { get; set; }
}