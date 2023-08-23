namespace Astrasend.Models;

/// <summary>
/// Статусы операции
/// </summary>
public enum StatusOperation
{
    /// <summary>
    /// Получен
    /// </summary>
    Received = 1,
    
    /// <summary>
    /// передан внешней системе
    /// </summary>
    TransferredToExternalSystem = 2,
    
    /// <summary>
    /// ошибка обработки
    /// </summary>
    Error = 3
}