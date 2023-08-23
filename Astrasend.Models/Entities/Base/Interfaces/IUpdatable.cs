namespace Astrasend.Models.Entities.Base.Interfaces;

/// <summary>
/// Управление датой обновления сущности в БД
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Дата последнего обновления
    /// </summary>
    DateTime UpdatedAt { get; }
        
    /// <summary>
    /// Установить дату
    /// </summary>
    void SetDate(DateTime date);
}
