using System.ComponentModel.DataAnnotations;

namespace Astrasend.Infrastructure.Settings;

/// <summary>
/// Класс настройки для взаимодействия с ApiClient
/// </summary>
public class ApiClientSettings
{
    /// <summary>
    /// Домен
    /// </summary>
    [Required]
    public string Url { get; set; } = string.Empty;
}