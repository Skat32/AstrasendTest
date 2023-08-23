using Astrasend.Models.ApiClient;

namespace Astrasend.Application.ApiClients;

/// <summary>
/// АПИ клиент взаимодействия с стороним сервисом
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Отправка сообщения 
    /// </summary>
    /// <param name="invoiceRequest"><see cref="InvoiceRequest"/></param>
    /// <param name="token"></param>
    Task SendInvoiceAsync(InvoiceRequest invoiceRequest, CancellationToken token);
}