using Intellegens.Commons.Results;
using Intellegens.Commons.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intellegens.Commons.Services
{
    public interface IRepositoryBase<TKey, TDto>
         where TDto : class, IDtoBase<TKey>
    {
        Task<Result<TDto>> FindById(TKey id);
        
        Task<Result<List<TDto>>> All();

        Task<Result<TDto>> Update(TDto entityDto);

        Task<Result<TDto>> Create(TDto entityDto);

        Task<Result> Delete(TKey id);

        Task<Result> Delete(TDto entityDto);

        public Task<(int? count, List<TDto> data)> Search(SearchRequest searchRequest, bool calculateTotalRecordCount = true);
    }
}