using Intellegens.Commons.Results;
using Intellegens.Commons.Search.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Intellegens.Commons.Api
{
    [Route("api/[controller]")]
    public abstract class CrudApiControllerAbstract<TKey, T> : ControllerBase
        where T : class
    {
        protected virtual void SetStatusCodeFromResult(Result result)
        {
            if (result.Errors.Any())
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        [HttpGet("{id}")]
        public abstract Task<ApiResult<T>> Get([FromRoute] TKey id);

        [HttpPost("{id}")]
        public abstract Task<ApiResult<T>> Update([FromBody] T data, [FromRoute] TKey id);

        [HttpDelete("{id}")]
        public abstract Task<ApiResult> Delete([FromRoute] TKey id);

        [HttpPost("")]
        public abstract Task<ApiResult<T>> Create([FromBody] T data);

        [HttpPost("search")]
        public abstract Task<ApiGridResult<T>> Search([FromBody] SearchRequest request);

        [HttpPost("indexof/{id}")]
        public abstract Task<ApiResult<int>> IndexOf([FromRoute] TKey id, [FromBody] SearchRequest request);
    }
}