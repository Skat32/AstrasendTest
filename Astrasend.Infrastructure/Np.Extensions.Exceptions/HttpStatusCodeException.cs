using System.Net;

namespace Astrasend.Infrastructure.Np.Extensions.Exceptions;

/// <summary>
/// Ошибка при отправке запроса в сторонние сервисы
/// </summary>
public class HttpStatusCodeException : CustomException
{
    /// <summary>
    /// код ошибки
    /// </summary>
    public int HttpStatusCode { get; }

    /// <inheritdoc />
    public HttpStatusCodeException(int httpStatusCode, string message, bool isErrorShouldReported = true) : base(message, isErrorShouldReported)
    {
        HttpStatusCode = httpStatusCode;
    }

    /// <inheritdoc />
    public HttpStatusCodeException(HttpStatusCode httpStatusCode, string message, bool isErrorShouldReported = true) : this((int)httpStatusCode, message, isErrorShouldReported) 
    { }
}
