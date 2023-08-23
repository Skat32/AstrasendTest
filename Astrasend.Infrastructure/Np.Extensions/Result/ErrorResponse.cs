using Microsoft.AspNetCore.Mvc;

namespace Astrasend.Infrastructure.Np.Extensions.Result;

/// <summary>
/// Абстрактный класс ошибок для команд и запросов
/// </summary>
public abstract class ErrorResponse
{
    /// <summary>
    /// Название ошибки
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Описание возникшей ошибки
    /// </summary>
    public string? Detail { get; private set; }

    /// <summary>ctor</summary>
    protected ErrorResponse(string? detail)
    {
        Detail = detail;
        Title = "";
    }

    /// <summary>ctor</summary>
    protected ErrorResponse(string? title, string? detail)
    {
        Detail = detail;
        Title = title;
    }
        
    /// <summary>
    /// ErrorResponse+StatusCode -> <see cref="ProblemDetails"/>
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    public ProblemDetails ToProblemDetails(int statusCode) =>
        new ProblemDetails
        {
            Type = GetType().Name,
            Title = Title ?? GetType().Name,
            Detail = Detail,
            Status = statusCode
        };
}