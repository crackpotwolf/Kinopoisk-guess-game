using System.ComponentModel.DataAnnotations.Schema;
using KinopoiskGuessGame.Data.Models._BaseEntities;
using Newtonsoft.Json;

namespace KinopoiskGuessGame.Data.Models;

/// <summary>
/// Ответ
/// </summary>
public class Answer : BaseEntity
{
    /// <summary>
    /// Guid Question
    /// </summary>
    [ForeignKey("Question")]
    public Guid QuestionGuid { get; set; }

    /// <summary>
    /// Регион
    /// </summary>
    [JsonIgnore]
    public virtual Question Question { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Верный или нет
    /// </summary>
    public bool IsCorrect { get; set; }
}