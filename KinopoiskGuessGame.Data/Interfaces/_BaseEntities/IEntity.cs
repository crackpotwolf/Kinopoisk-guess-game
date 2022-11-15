﻿namespace KinopoiskGuessGame.Data.Interfaces._BaseEntities;

/// <summary>
/// Базовый интерфейс
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime DateCreate { get; set; }

    /// <summary>
    /// Дата обновления записи
    /// </summary>
    public DateTime DateUpdate { get; set; }

    /// <summary>
    /// Обновление дат перед сохранением в БД
    /// </summary>
    /// <param name="now"></param>
    public void UpdateBeforeSave(DateTime now);
}