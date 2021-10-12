using Intellegens.Commons.Search.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Search
{
    /// <summary>
    /// This service uses filters based on TDto to filter IQueryable<TEntity> and always return TDto objects (single or list, ...)
    ///
    /// To avoid all issues when filtering/ordering IQueryables which are AutoMapped from entity to some dto, this service uses
    /// SearchRequest made for TDto, translates it, filters IQueryable<TEntity> and maps it to TDto after doing all EF operations
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public interface IEntity2DtoSearchService<TEntity, TDto>
        where TEntity : class, new()
        where TDto : class, new()
    {
        Task<int> IndexOf(string keyColumn, IQueryable<TEntity> sourceData, TDto dto, SearchRequest searchRequest);
        Task<List<TDto>> Search(IQueryable<TEntity> sourceData, SearchRequest searchRequest);
        Task<(int count, List<TDto> data)> SearchAndCount(IQueryable<TEntity> sourceData, SearchRequest searchRequest);
    }
}