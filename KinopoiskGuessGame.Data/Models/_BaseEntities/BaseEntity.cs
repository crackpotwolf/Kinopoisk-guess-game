using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using KinopoiskGuessGame.Data.Interfaces._BaseEntities;

namespace KinopoiskGuessGame.Data.Models._BaseEntities;

/// <summary>
/// Базовый класс
/// </summary>
public class BaseEntity : Entity, IBaseEntity
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    [Key]
    public virtual Guid Guid { get; set; }

    /// <summary>
    /// Флаг удаления
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public bool IsDelete { get; set; }
}