using Intellegens.Commons.Results;
using Intellegens.Commons.Search.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intellegens.Commons.Services
{
    /// <summary>
    /// When constructing services, two basic types should be used:
    /// - entity used by ORM
    /// - dto used by repository
    /// Entities should never be exposed beyond service layer, and each entity can have one or more DTOs.
    /// For example, User entity could have one DTO for personal data, and other for security data.
    /// This is usually used in combination with AutoMapper to make mapping between entity and DTO easier
    /// </summary>
    /// <typeparam name="TKey">DTO key type</typeparam>
    /// <typeparam name="TDto">DTO type</typeparam>
    public interface IRepositoryBase<TKey, TDto>
         where TDto : class, IDtoBase<TKey>
    {
        public Task<Result<TDto>> FindById(TKey id);

        public Task<Result<List<TDto>>> All();

        public Task<Result<TDto>> Update(TDto entityDto);

        public Task<Result<TDto>> Create(TDto entityDto);

        public Task<Result> Delete(TKey id);

        public Task<Result> Delete(TDto entityDto);

        public Task<(int? count, List<TDto> data)> Search(SearchRequest searchRequest, bool calculateTotalRecordCount = true);

        /// <summary>
        /// In given search request, find number of first row with given entity Id
        /// </summary>
        /// <param name="searchRequest"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<Result<int>> IndexOf(SearchRequest searchRequest, TKey id);
    }
}