using Intellegens.Commons.Results;
using System.Collections.Generic;

namespace Intellegens.Commons.Mvc.Models
{
    /// <summary>
    /// ApiResult class should be used by all api endpoints - directly or inherited in some way.
    /// Represents standard API response
    /// </summary>
    public class ApiResult
    {
        public ApiResult()
        {
        }

        public ApiResult(bool success)
        {
            Success = success;
        }

        public IList<ResultError> Errors { get; set; } = new List<ResultError>();

        public bool Success { get; set; }

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

        public static ApiResult SuccessResult()
            => new ApiResult { Success = true };
    }
}