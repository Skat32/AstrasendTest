namespace Astrasend.Infrastructure.Np.Extensions.Result;

/// <summary>
/// Класс-обертка Result для CQRS
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : ResultBase
{
    /// <summary>
    /// Результат
    /// </summary>
    public T Data { get; private set; }

    /// <summary>
    /// Создание успешного ответа
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Result<T> Success(T data) => new() {Data = data};

    /// <summary>
    /// Создание ответа с ошибкой
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Error(ErrorResponse error) => new() {ErrorResponse = error};

    /// <summary>
    /// Implicit ErrorResponse to Result<T/>
    /// </summary>
    /// <param name="errorResponse"></param>
    /// <returns></returns>
    public static implicit operator Result<T>(ErrorResponse errorResponse) => new() {ErrorResponse = errorResponse};

    /// <summary>
    /// Implicit T to Result<T/>
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static implicit operator Result<T>(T data) => new() {Data = data};

    private Result() {}
}