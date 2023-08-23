using System.Net.Http.Formatting;

namespace Astrasend.Infrastructure;

/// <summary>
/// Класс расширение для отправки xml сообщений с использованием HttpClient
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Sends a POST request as an asynchronous operation to the specified Uri with the given <paramref name="value" /> serialized
    /// as XML.
    /// </summary>
    public static Task<HttpResponseMessage> PostAsXmlWithSerializerAsync<T>
        (this HttpClient client, string requestUrl, T value, CancellationToken token)
     => client.PostAsync(requestUrl, value,
         new XmlMediaTypeFormatter { UseXmlSerializer = true }, token);
}