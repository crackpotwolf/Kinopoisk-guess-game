using KinopoiskGuessGame.Data.Models._BaseEntities;

namespace KinopoiskGuessGame.Data.Models;

/// <summary>
/// Вопрос
/// </summary>
public class Question : BaseEntity
{
    /// <summary>
    /// Номер игры
    /// </summary>
    public int GameId { get; set; }
    
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
 
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; }
}