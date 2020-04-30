using System.Collections.Generic;

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
        {
            var apiResult = new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors,
                Data = new List<T> { }
            };

            if (result.Data != null)
                apiResult.Data.Add(result.Data);

            return apiResult;
        }

        public static ApiResult<T> ToApiResult<T>(this Result<List<T>> result)
        {
            var apiResult = new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors,
                Data = new List<T> { }
            };

            if (result.Data != null)
                apiResult.Data.AddRange(result.Data);

            return apiResult;
        }
    }
}