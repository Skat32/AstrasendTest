using Polly;

namespace Astrasend.Infrastructure.Np.Policy.ClientPolicy.Interfaces;

/// <summary>
/// Настройка Polly для повторных попыток отправки запросов через HttpClient
/// </summary>
public interface IClientPolicy
{
    /// <summary>
    /// Отправить Http request с использованием policy
    /// </summary>
    /// <param name="func">Метод httpClient'а</param>
    /// <returns><see cref="HttpResponseMessage"/></returns>
    public Task<HttpResponseMessage> UsePolicy(Func<Task<HttpResponseMessage>> func);
    
    /// <summary>
    /// Получить Policy использую билдер
    /// </summary>
    public IAsyncPolicy<HttpResponseMessage> GetPolicy(PolicyBuilder<HttpResponseMessage> policy);
}
