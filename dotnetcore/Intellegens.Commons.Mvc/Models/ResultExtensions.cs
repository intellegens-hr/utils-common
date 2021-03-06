﻿using Intellegens.Commons.Results;
using System.Collections.Generic;

namespace Intellegens.Commons.Mvc.Models
{
    /// <summary>
    /// Various extensions which transform Result to ApiResult/ApiResult<T>
    /// </summary>
    public static class ResultExtensions
    {
        public static ApiResult ToApiResult(this Result result)
            => new ApiResult
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
                apiResult.Data = new T[] { result.Data };

            return apiResult;
        }

        public static ApiResult<T> ToApiResult<T>(this Result<IEnumerable<T>> result)
        {
            var apiResult = new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors
            };

            if (result.Data != null)
                apiResult.Data = result.Data;

            return apiResult;
        }

        public static ApiResult<T> ToTypedResult<T>(this ApiResult result)
            => new ApiResult<T>
            {
                Success = result.Success,
                Errors = result.Errors
            };
    }
}