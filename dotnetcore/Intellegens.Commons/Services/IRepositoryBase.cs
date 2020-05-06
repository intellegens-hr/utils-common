using Intellegens.Commons.Results;
using Intellegens.Commons.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intellegens.Commons.Services
{
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

        public Task<int> IndexOf(SearchRequest searchRequest, TKey id);
    }
}