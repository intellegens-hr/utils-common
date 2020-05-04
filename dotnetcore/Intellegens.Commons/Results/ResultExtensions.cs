using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Results
{
    public static class ResultExtensions
    {
        public static ApiResult ToApiResult(this Result result)
            => new ApiResult
            {
                Success = result.Success,
                Errors = result.Errors
            };

        public static ApiResult<T> ToTypedResult<T>(this ApiResult result)
            => new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors
            };

        public static ApiResult<T> ToApiResult<T>(this Result<T> result)
            => new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors,
                Data = new List<T> { result.Data }
            };
    }
}