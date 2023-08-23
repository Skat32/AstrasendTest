namespace Astrasend.Infrastructure.Np.Extensions.Exceptions;

/// <summary>
/// Базовый класс для кастомных exceptions
/// </summary>
public abstract class CustomException : Exception
{
    /// <ctor/>
    protected CustomException(string message, bool isErrorShouldReported = true) : base(message)
    {
        IsErrorShouldReported = isErrorShouldReported;
    }

    /// <summary>
    /// Признак того, что ошибка должна быть отправлена в телеграмм
    /// </summary>
    public bool IsErrorShouldReported { get; }
}