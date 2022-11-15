using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using KinopoiskGuessGame.Common.Extensions;
using KinopoiskGuessGame.Data.Interfaces.Repositories;
using KinopoiskGuessGame.Data.Models._BaseEntities;

namespace KinopoiskGuessGame.Common.Api;

/// <summary>
/// Базовый контроллер выполняющий CRUD операции
/// </summary>
[ApiController]
public abstract class BaseCRUDController<T> : ControllerBase
    where T : BaseEntity
{
    /// <summary>
    /// Репозиторий
    /// </summary>
    private readonly IBaseEntityRepository<T> _repository;
    private readonly ILogger<IndexModel> _logger;
    private readonly IMapper _mapper;

    /// <inheritdoc />
    protected BaseCRUDController(IBaseEntityRepository<T> repository,
        ILogger<IndexModel> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Получение списка записей
    /// </summary>
    protected virtual IQueryable<T> List => _repository.GetListQuery();

    /// <summary>
    /// Получение списка всех записей
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public virtual IActionResult ListEntities()
    {
        var result = List.OrderBy(p => p.DateCreate)
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Получение списка части записей
    /// </summary>
    /// <param name="limit">Количество записей в ответе (выбираются первые {limit} записей из выборки после {numberSkip} элементов)
    /// <br/><br/>
    /// <b>Max value = 1000</b>
    /// </param>
    /// <param name="numberSkip">Пропускается первых {numberSkip} записей</param>
    /// <returns></returns>
    [HttpGet("piece")]
    public virtual IActionResult ListEntitiesPiece(int limit = 1000, int numberSkip = 0)
    {
        if (limit is < 0 or > 1000) limit = 1000;
        if (numberSkip < 0) numberSkip = 0;

        var result = List.OrderBy(p => p.DateCreate)
            .Skip(numberSkip)
            .Take(limit)
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Получение записи
    /// </summary>
    /// <param name="guid">Идентификатор записи</param>
    /// <returns>Запись</returns>
    /// <response code="404">Если записи с данным идентификатором не существует</response>   
    [HttpGet("{guid:guid}")]
    public virtual IActionResult Get(Guid guid)
    {
        var entity = List.FirstOrDefault(p => p.Guid == guid);

        if (entity == null) return NotFound("Guid не найден");
        return Ok(entity);
    }

    /// <summary>
    /// Добавление записи
    /// </summary>
    /// <param name="model">Запись</param>
    /// <response code="400">Запись не прошла валидацию</response>
    /// <response code="500">При добавлении записи произошла ошибка на сервере</response>   
    /// <returns>Добавленная запись</returns>
    [HttpPost]
    [SwaggerResponse(200, "Запись успешно добавлена. Содержит информацию о добавленной записи", typeof(BaseEntity))]
    [SwaggerResponse(500, "Ошибка при добавлении записи")]
    public virtual IActionResult Add(T model)
    {
        if (_repository.Add(model).Guid != Guid.Empty)
            return Ok(model);
        return StatusCode(500, "Произошла ошибка при добавлении записи");
    }

    /// <summary>
    /// Добавление записей
    /// </summary>
    /// <param name="models">Записи</param>
    /// <response code="400">Записи не прошли валидацию</response>
    /// <response code="500">При добавлении записи произошла ошибка на сервере</response>   
    /// <returns>Добавленная запись</returns>
    [HttpPost("range")]
    [SwaggerResponse(200, "Записи успешно добавлены. Содержит список добавленных записей", typeof(List<BaseEntity>))]
    [SwaggerResponse(500, "Произошла ошибка при добавлении записей")]
    public virtual IActionResult AddRange(List<T> models)
    {
        if (_repository.AddRange(models).All(p => p.Guid != Guid.Empty))
            return Ok(models);

        return StatusCode(500, "Произошла ошибка при добавлении записи");
    }

    /// <summary>
    /// Удаление записи
    /// </summary>
    /// <param name="guid">Идентификатор записи</param>
    /// <response code="500">При удалении записи произошла ошибка</response>   
    /// <returns></returns>
    [HttpDelete("{guid}")]
    [HttpDelete("remove/{guid}")]
    [SwaggerResponse(200, "Запись успешно удалена")]
    [SwaggerResponse(404, "Запись не найдена")]
    [SwaggerResponse(500, "Произошла ошибка при удаленнии записи")]
    public virtual IActionResult RemoveByGuid(Guid guid)
    {
        var entity = _repository.Get(guid);
        if (entity == null) return NotFound();

        if (_repository.Remove(guid))
            return Ok();

        return StatusCode(500, "Произошла ошибка при удалении записи");
    }

    /// <summary>
    /// Удаление записей
    /// </summary>
    /// <param name="guids">Идентификаторы записей</param>
    /// <response code="500">При удалении записей произошла ошибка</response>   
    /// <returns></returns>
    [HttpDelete("range")]
    [SwaggerResponse(200, "Записи успешно удалены. В ответе список Guid записей, которые были удалены", typeof(List<Guid>))]
    [SwaggerResponse(400, "Не указаны Guid записей, которые необходимо удалить")]
    [SwaggerResponse(500, "Произошла ошибка при удалении записей")]
    public virtual IActionResult RemoveRangeByGuid(List<Guid> guids)
    {
        if (guids.Count == 0) return BadRequest();

        if (_repository.RemoveRange(guids))
            return Ok();

        return StatusCode(500, "Произошла ошибка при удалении записи");
    }

    /// <summary>
    /// Обновление записи
    /// </summary>
    /// <param name="model">Обновленная запись</param>
    /// <response code="400">Запись не найдена</response>   
    /// <response code="500">При удалении записи произошла ошибка</response>   
    /// <returns></returns>
    [HttpPut]
    [SwaggerResponse(200, "Запись успешно обновлена. Содержит информацию об обновленной записи", typeof(BaseEntity))]
    [SwaggerResponse(404, "Запись не найдена")]
    [SwaggerResponse(500, "При обновлении записи произошла ошибка")]
    public virtual IActionResult Update(T model)
    {
        var fromDb = _repository.Get(model.Guid);
        if (fromDb == null) return BadRequest("Запись с заданным идентификатором не найдена");

        var res = model.UpdateEntity(_mapper, fromDb);

        if (_repository.Update(res))
            return Ok(res);

        return StatusCode(500, "Не удалось обновить запись");
    }

    /// <summary>
    /// Обновление записей
    /// </summary>
    /// <param name="models">Обновленные записи</param>
    /// <response code="400">Записи не найдены</response>   
    /// <response code="500">При удалении записей произошла ошибка</response>   
    /// <returns></returns>
    [HttpPut("range")]
    [SwaggerResponse(200, "Записи успешно обновлены. Содержит информацию об обновленных записях", typeof(List<BaseEntity>))]
    [SwaggerResponse(400, "Нет записей для обновления")]
    [SwaggerResponse(500, "При обновлении записей произошла ошибка")]
    public virtual IActionResult UpdateRange(List<T> models)
    {
        if (models.Count == 0) return BadRequest();

        var fromDb = _repository.GetListQuery()
            .Where(p => models.Select(x => x.Guid).Contains(p.Guid))
            .ToList();

        var notFoundGuids = models.Select(p => p.Guid)
            .Except(fromDb.Select(p => p.Guid))
            .ToList();

        if (notFoundGuids.Count != 0) return BadRequest(notFoundGuids);

        models = fromDb.Join(models,
                d => d.Guid,
                m => m.Guid,
                (d, m) => m.UpdateEntity(_mapper, d))
            .ToList();

        if (_repository.UpdateRange(models))
            return Ok(models);

        return StatusCode(500, "Не удалось обновить запись");
    }
}