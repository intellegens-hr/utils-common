using System.Collections.Generic;

namespace Intellegens.Commons.Results
{
    /// <summary>
    /// ApiResult class should be used by all api endpoints - directly or inherited in some way.
    /// Represents standard API response
    /// </summary>
    public class ApiResult
    {
        public static ApiResult SuccessResult()
           => new ApiResult { Success = true };

        public static ApiResult ErrorResult(string errorCode)
        {
            var apiResult = new ApiResult(false);
            apiResult.Errors.Add(ResultError.FromErrorCode(errorCode));

            return apiResult;
        }

        public static ApiResult ErrorResult(ResultError error)
        {
            var apiResult = new ApiResult(false);
            apiResult.Errors.Add(error);

            return apiResult;
        }

        public ApiResult()
        {
        }

        public ApiResult(bool success)
        {
            Success = success;
        }

        public bool Success { get; set; }
        public List<ResultError> Errors { get; set; } = new List<ResultError>();
    }

    /// <summary>
    /// Api result containing data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResult<T> : ApiResult
    {
        public static new ApiResult<T> SuccessResult()
            => new ApiResult<T> { Success = true };

        public static ApiResult<T> SuccessResultWithData(T data)
            => new ApiResult<T>(true, data);

        public static ApiResult<T> SuccessResultWithData(List<T> data)
            => new ApiResult<T>(true, data);

        public static new ApiResult<T> ErrorResult(string errorCode)
        {
            var apiResult = new ApiResult<T>();
            apiResult.Success = false;
            apiResult.Errors.Add(ResultError.FromErrorCode(errorCode));

            return apiResult;
        }

        public ApiResult() : base()
        {
        }

        public ApiResult(bool success, T data) : base(success)
        {
            Data.Add(data);
        }

        public ApiResult(bool success, List<T> data) : base(success)
        {
            Data = data;
        }

        public List<T> Data { get; set; } = new List<T>();
    }
}