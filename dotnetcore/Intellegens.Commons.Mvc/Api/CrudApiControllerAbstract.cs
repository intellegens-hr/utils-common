using Intellegens.Commons.Mvc.Models;
using Intellegens.Commons.Results;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Intellegens.Commons.Api
{
    [Route("api/[controller]")]
    public abstract class CrudApiControllerAbstract<TEntity, TDto> : ReadApiControllerAbstract<TEntity, TDto>
        where TEntity : class
        where TDto : class, IDtoBase<int>
    {
        public CrudApiControllerAbstract(IRepositoryBase<TDto> repositoryBase) : base(repositoryBase)
        {
        }
    }

    [Route("api/[controller]")]
    public abstract class CrudApiControllerAbstract<TEntity, TDto, TKey> : ReadApiControllerAbstract<TEntity, TDto, TKey>
        where TEntity : class
        where TDto : class, IDtoBase<TKey>
    {
        public CrudApiControllerAbstract(IRepositoryBase<TDto, TKey> repositoryBase) : base(repositoryBase)
        {
        }

        [HttpPost("")]
        public virtual async Task<ApiResult<TDto>> Create([FromBody] TDto data)
        {
            var result = await repository.Create(data);
            SetStatusCodeFromResult(result);
            return result.ToApiResult<TDto>();
        }

        [HttpDelete("{id}")]
        public virtual async Task<ApiResult> Delete([FromRoute] TKey id)
        {
            var result = await repository.Delete(id);
            SetStatusCodeFromResult(result);
            return result.ToApiResult();
        }

        [HttpPut("{id}")]
        public virtual async Task<ApiResult<TDto>> Update([FromBody] TDto data, [FromRoute] TKey id)
        {
            if (!data.GetIdValue().Equals(id))
                return ApiResult.ErrorResult(ResultError.FromErrorCode(CommonErrorCodes.RouteAndPayloadIdMismatch)).ToTypedResult<TDto>();

            var result = await repository.Update(data);
            SetStatusCodeFromResult(result);
            return result.ToApiResult<TDto>();
        }
    }
}