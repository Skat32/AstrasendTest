namespace Astrasend.Models.Entities.Base.Interfaces;

/// <summary>
/// интерфейс управления мягким удалением
/// </summary>
public interface ISoftDeletableEntity
{
    /// <summary>
    /// Признак удалена ли запись в базе 
    /// </summary>
    bool IsDeleted { get; }
        
    /// <summary>
    /// Удалить
    /// </summary>
    void Delete();
}
