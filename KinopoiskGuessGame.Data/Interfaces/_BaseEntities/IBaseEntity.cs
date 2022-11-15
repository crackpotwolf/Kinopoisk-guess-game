using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace KinopoiskGuessGame.Data.Interfaces._BaseEntities;

/// <summary>
/// Базовый интерфейс
/// </summary>
public interface IBaseEntity : IEntity
{
    /// <summary>
    /// Уникальный идентификатор
    /// </summary>
    [Key]
    Guid Guid { get; set; }

    /// <summary>
    /// Флаг удаления
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    bool IsDelete { get; set; }
}