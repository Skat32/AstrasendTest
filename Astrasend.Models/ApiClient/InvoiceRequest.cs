using System.Xml.Serialization;

namespace Astrasend.Models.ApiClient;

/// <summary>
/// Объект отправки сообщения в сторонний сервис
/// </summary>
[XmlRoot("invoice_payment")]
[XmlType("invoice_payment")]
public class InvoiceRequest
{
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    [XmlElement(ElementName = "id")]
    public ulong Id { get; set; }

    /// <summary>
    /// Номер счета отправителя
    /// </summary>
    [XmlElement(ElementName = "debit")]
    public string DebitAccountNumber { get; set; }

    /// <summary>
    /// Номер счета получателя
    /// </summary>
    [XmlElement(ElementName = "credit")]
    public string CreditAccountNumber { get; set; }

    /// <summary>
    /// Сумма перевода
    /// </summary>
    [XmlElement(ElementName = "amount")]
    public decimal DebitAmount { get; set; }

    /// <summary>
    /// Валюта перевода
    /// </summary>
    [XmlElement(ElementName = "currency")]
    public string Currency { get; set; }

    /// <summary>
    /// Комментарий к платежу
    /// </summary>
    [XmlElement(ElementName = "details")]
    public string Details { get; set; }

    /// <summary>
    /// Дополнительные аргументы?
    /// </summary>
    [XmlElement(ElementName = "pack")]
    public SerializableDictionary<string, string>? Pack { get; set; }
}