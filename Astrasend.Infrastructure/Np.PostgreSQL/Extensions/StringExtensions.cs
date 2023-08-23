using System.Text.RegularExpressions;

namespace Astrasend.Infrastructure.Np.PostgreSQL.Extensions;

/// <summary>
/// Расширение для типа String
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Преобразование в snake_case
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = Regex.Match(input, @"^_+");
        return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
}
