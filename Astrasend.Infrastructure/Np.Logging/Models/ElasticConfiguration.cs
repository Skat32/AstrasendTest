namespace Astrasend.Infrastructure.Np.Logging.Models;

/// <summary>
/// Настройки для подключения к Elastic
/// </summary>
public class ElasticConfiguration
{
    /// <summary>
    /// Ссылка
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Логин
    /// </summary>
    public string Login { get; set; }

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; }
}
