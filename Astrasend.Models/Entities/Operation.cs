using System.ComponentModel.DataAnnotations.Schema;
using Astrasend.Models.Entities.Base;

namespace Astrasend.Models.Entities;

/// <summary>
/// Полученная операция
/// </summary>
public class Operation : BaseEntity
{
    /// <summary>
    /// Полученное сообщение
    /// <remarks>Используется тип object тк дальнейших действий с полем не планируется</remarks>
    /// </summary>
    [Column(TypeName = "jsonb")]
    public object Message { get; private set; }

    /// <summary>
    /// Статус операции
    /// </summary>
    public StatusOperation Status { get; private set; }

    /// ctor
    /// <param name="message"><see cref="Message"/></param>
    public Operation(object message)
    {
        Message = message;
        Status = StatusOperation.Received;
    }

    /// <summary>
    /// Переданно на внешнюю систему
    /// </summary>
    public void TransferredToExternalSystem()
    {
        Status = StatusOperation.TransferredToExternalSystem;
    }

    /// <summary>
    /// Ошибка проведения операции
    /// </summary>
    public void Error()
    {
        Status = StatusOperation.Error;
    }
}