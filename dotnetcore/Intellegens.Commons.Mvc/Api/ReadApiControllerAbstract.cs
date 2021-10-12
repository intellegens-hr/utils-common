using Intellegens.Commons.Mvc.Models;
using Intellegens.Commons.Results;
using Intellegens.Commons.Search.Models;
using Intellegens.Commons.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Api
{
    [Route("api/[controller]")]
    public abstract class ReadApiControllerAbstract<TEntity, TDto> : ReadApiControllerAbstract<TEntity, TDto, int>
        where TEntity : class
        where TDto : class, IDtoBase<int>
    {
        public ReadApiControllerAbstract(IRepositoryBase<TDto> repositoryBase) : base(repositoryBase)
        {
        }
    }

    [Route("api/[controller]")]
    public abstract class ReadApiControllerAbstract<TEntity, TDto, TKey> : ControllerBase
        where TEntity : class
        where TDto : class, IDtoBase<TKey>
    {
        protected readonly IRepositoryBase<TDto, TKey> repository;

        public ReadApiControllerAbstract(IRepositoryBase<TDto, TKey> repositoryBase)
        {
            this.repository = repositoryBase;
        }

        [HttpGet("{id}")]
        public virtual async Task<ApiResult<TDto>> Get([FromRoute] TKey id)
        {
            var result = await repository.FindById(id);
            SetStatusCodeFromResult(result);
            return result.ToApiResult<TDto>();
        }

        [HttpPost("indexof/{id}")]
        public virtual async Task<ApiResult<int>> IndexOf([FromRoute] TKey id, [FromBody] SearchRequest request)
        {
            var indexResult = await repository.IndexOf(request, id);
            return indexResult.ToApiResult<int>();
        }

        [HttpPost("search")]
        public virtual async Task<ApiGridResult<TDto>> Search([FromBody] SearchRequest request)
        {
            var data = await repository.Search(request);
            return new ApiGridResult<TDto>
            {
                Data = data.data,
                Metadata = new SearchResponseMetadata
                {
                    Request = request,
                    TotalRecordCount = data.count
                },
                Success = true
            };
        }

        protected virtual void SetStatusCodeFromResult(Result result)
        {
            if (result.Errors.Any() && result.Errors.All(x => x.Code == CommonErrorCodes.NotFound))
            {
                SetStatusCode(StatusCodes.Status404NotFound);
            }
            else if (result.Errors.Any())
                SetStatusCode(StatusCodes.Status400BadRequest);
        }

        protected virtual void SetStatusCode(int statusCode)
        {
            HttpContext.Response.StatusCode = statusCode;
        }
    }
}