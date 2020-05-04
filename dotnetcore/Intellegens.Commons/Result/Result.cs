using System.Collections.Generic;

namespace Intellegens.Commons.Result
{
    public class Result
    {
        #region Static initializeers

        public static Result SuccessResult()
            => new Result { Success = true };

        public static Result ErrorResult(string errorCode)
        {
            var apiResult = new Result(false);
            apiResult.Errors.Add(ResultError.FromErrorCode(errorCode));

            return apiResult;
        }

        public static Result ErrorResult(ResultError error)
        {
            var apiResult = new Result(false);
            apiResult.Errors.Add(error);

            return apiResult;
        }

        #endregion Static initializeers

        public Result()
        {
        }

        public Result(bool success)
        {
            Success = success;
        }

        public bool Success { get; set; }
        public List<ResultError> Errors { get; set; } = new List<ResultError>();
    }
}