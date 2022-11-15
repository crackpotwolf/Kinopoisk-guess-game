using System.Linq.Expressions;
using KinopoiskGuessGame.Data.Interfaces._BaseEntities;

namespace KinopoiskGuessGame.Data.Interfaces.Repositories;

/// <summary>
/// Интерфейс базового репозитория
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IBaseEntityRepository<T> where T : IBaseEntity
{
    T Get(Guid guid);
    T Add(T model);
    IEnumerable<T> AddRange(IEnumerable<T> models);
    bool Update(T models);
    bool UpdateRange(IEnumerable<T> models);
    bool Remove(T model);
    bool Remove(Guid Guid);
    bool RemoveRange(IEnumerable<T> models);
    bool RemoveRange(IEnumerable<Guid> guids);
    bool Delete(T model);
    bool DeleteRange(IEnumerable<T> models);
    bool DeleteRange(IEnumerable<Guid> guids);
    IQueryable<T> GetListQuery();
    IQueryable<T> GetListQueryWithDeleted();
    List<T> GetList();
    IEnumerable<T> GetListWithDeleted();
    bool Any(Expression<Func<T, bool>> func);
    T FirstOrDefault(Expression<Func<T, bool>> func);
}