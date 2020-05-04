using Intellegens.Commons.Results;
using Intellegens.Commons.Search;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intellegens.Commons.Services
{
    public interface IRepositoryBase<TDto>
         where TDto : class, IDtoBase
    {
        Task<Result<TDto>> FindById(Guid id);

        Task<Result<TDto>> Update(TDto entityDto);

        Task<Result<TDto>> Create(TDto entityDto);

        Task<Result> Delete(Guid id);

        Task<Result> Delete(TDto entityDto);

        public Task<(int? count, List<TDto> data)> Search(SearchRequest searchRequest, bool calculateTotalRecordCount = true);
    }
}