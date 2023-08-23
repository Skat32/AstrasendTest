using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.Np.PostgreSQL.Settings;

/// <summary>
/// Файл настройки БД
/// </summary>
public class DbSettings
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required]
    public string User { get; set; }

    /// <summary>
    /// Пароль
    /// </summary>
    [Required]
    public string Password { get; set; }

    /// <summary>
    /// Хост
    /// </summary>
    [Required]
    public string Host { get; set; }

    /// <summary>
    /// Порт
    /// </summary>
    [Required]
    public string Port { get; set; }

    /// <summary>
    /// Имя бд
    /// </summary>
    [Required]
    public string DbName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int CommandTimeout { get; set; }

    /// <summary>
    /// Максимальное кол-во коннектов в пуле Npsql
    /// </summary>
    public int MaxPoolSize { get; set; } = 100;

    public string Schema { get; set; } = "public";
}
