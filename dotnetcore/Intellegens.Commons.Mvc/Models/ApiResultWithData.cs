using Intellegens.Commons.Results;
using System.Collections.Generic;
using System.Linq;

namespace Intellegens.Commons.Mvc.Models
{
    /// <summary>
    /// Api result containing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        public ApiResult() : base()
        {
        }

        public ApiResult(bool success, T data) : base(success)
        {
            Data = new T[] { data };
        }

        public ApiResult(bool success, IEnumerable<T> data) : base(success)
        {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

        public static new ApiResult<T> ErrorResult(string errorCode)
        {
            var apiResult = new ApiResult<T>();
            apiResult.Success = false;
            apiResult.Errors.Add(ResultError.FromErrorCode(errorCode));

            return apiResult;
        }

        public static new ApiResult<T> SuccessResult()
            => new ApiResult<T> { Success = true };

        public static ApiResult<T> SuccessResultWithData(T data)
            => new ApiResult<T>(true, data);

        public static ApiResult<T> SuccessResultWithData(IEnumerable<T> data)
            => new ApiResult<T>(true, data);
    }
}