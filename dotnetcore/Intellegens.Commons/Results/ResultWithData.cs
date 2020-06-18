using System.Collections.Generic;

namespace Intellegens.Commons.Results
{
    /// <summary>
    /// Result class with data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T> : Result
    {
        public static Result<T> SuccessDataResult(T data)
            => new Result<T>(true, data);

        public static Result<T> ErrorDataResult(string errorCode)
        {
            var apiResult = new Result<T>();
            apiResult.Success = false;
            apiResult.Errors.Add(ResultError.FromErrorCode(errorCode));

            return apiResult;
        }

        public static Result<T> ErrorDataResult(ResultError error)
        {
            var apiResult = new Result<T>();
            apiResult.Success = false;
            apiResult.Errors.Add(error);

            return apiResult;
        }

        public Result() : base()
        {
        }

        public Result(bool success, T data) : base(success)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}