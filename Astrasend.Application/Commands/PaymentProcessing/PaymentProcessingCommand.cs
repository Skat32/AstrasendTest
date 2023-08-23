using Astrasend.Infrastructure.Np.Extensions.Result;
using MediatR;
using Newtonsoft.Json;

namespace Astrasend.Application.Commands.PaymentProcessing;

/// <summary>
/// Обработка платежа
/// </summary>
public class PaymentProcessingCommand : IRequest<Result<Unit>>
{
    /// <summary>
    /// Информация о платеже
    /// </summary>
    [JsonProperty("request")]
    public Request Request { get; private set; }
    
    /// <summary>
    /// Информация о плательщике
    /// </summary>
    [JsonProperty("debitPart")]
    public UserPart DebitPart { get; private set; }
    
    /// <summary>
    /// Информация о получателе
    /// </summary>
    [JsonProperty("creditPart")]
    public UserPart CreditPart { get; private set; }
    
    /// <summary>
    /// Комментарий к платежу
    /// </summary>
    [JsonProperty("details")]
    public string Details { get; private set; }
    
    /// <summary>
    /// Дата проведения операции
    /// </summary>
    [JsonProperty("bankingDate")]
    public string BankingDate { get; private set; }
    
    /// <summary>
    /// Дополнительные данные
    /// </summary>
    [JsonProperty("attributes")]
    public Attributes Attributes { get; private set; }
}

/// <summary>
/// Данные атрибута
/// </summary>
public class Attribute
{
    /// <summary>
    /// Ключ
    /// </summary>
    [JsonProperty("code")]
    public string Key { get; private set; }
    
    /// <summary>
    /// Значение
    /// </summary>
    [JsonProperty("attribute")]
    public string Value { get; private set; }
}

/// <summary>
/// Дополнительные данные
/// </summary>
public class Attributes
{
    /// <summary>
    /// Список дополнительных аттрибутов
    /// </summary>
    [JsonProperty("attribute")]
    public List<Attribute>? Attribute { get; private set; }
}

/// <summary>
/// Информация о плательщике
/// </summary>
public class UserPart
{
    /// <summary>
    /// Номер соглашения
    /// </summary>
    [JsonProperty("agreementNumber")]
    public string AgreementNumber { get; private set; }
    
    /// <summary>
    /// Номер счета
    /// </summary>
    [JsonProperty("accountNumber")]
    public string AccountNumber { get; private set; }
    
    /// <summary>
    /// Сумма перевода
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; private set; }
    
    /// <summary>
    /// Валюта перевода
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; private set; }
    
    /// <summary>
    /// Доп информация о платеже
    /// </summary>
    [JsonProperty("attributes")]
    public Attributes Attributes { get; private set; }
}

/// <summary>
/// Дополнительная информация о документе
/// </summary>
public class Document
{
    /// <summary>
    /// Идентификатор документа
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; private set; }
    
    /// <summary>
    /// Тип документа
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; private set; }
}

/// <summary>
/// Информация о платеже
/// </summary>
public class Request
{
    /// <summary>
    /// Идентификатор платежа
    /// </summary>
    [JsonProperty("id")]
    public ulong Id { get; private set; }
    
    /// <summary>
    /// Дополнительная информация о документе
    /// </summary>
    [JsonProperty("document")]
    public Document Document { get; private set; }
}

