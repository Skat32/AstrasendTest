using Astrasend.Infrastructure;
using Astrasend.Models.ApiClient;

namespace Astrasend.Application.ApiClients;

/// <inheritdoc />
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    /// ctor
    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task SendInvoiceAsync(InvoiceRequest invoiceRequest, CancellationToken token)
    {
        const string url = "api/v1/invoice";
        
        var result = await _httpClient.PostAsXmlWithSerializerAsync(url, invoiceRequest, token);

        result.EnsureSuccessStatusCode();
    }
}