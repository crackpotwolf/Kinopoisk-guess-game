using AutoMapper;
using System.ComponentModel;
using KinopoiskGuessGame.Data.Interfaces._BaseEntities;
using Newtonsoft.Json;

namespace KinopoiskGuessGame.Data.Models._BaseEntities;

/// <summary>
/// Базовый класс
/// </summary>
public class Entity : IEntity
{
    /// <inheritdoc />
    public Entity()
    {
        DateCreate = DateTime.UtcNow;
        DateUpdate = DateCreate;
    }

    /// <summary>
    /// Дата создания записи
    /// </summary>
    [ReadOnly(true)]
    [JsonIgnore]
    [IgnoreMap]
    public virtual DateTime DateCreate { get; set; }

    /// <summary>
    /// Дата обновления записи
    /// </summary>
    [ReadOnly(true)]
    [JsonIgnore]
    [IgnoreMap]
    public virtual DateTime DateUpdate { get; set; }

    /// <summary>
    /// Обновление дат перед сохранением в БД
    /// </summary>
    /// <param name="now"></param>
    public void UpdateBeforeSave(DateTime now)
    {
        DateUpdate = now;
    }
}