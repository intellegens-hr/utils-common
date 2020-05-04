namespace Intellegens.Commons.Result
{
    public class ResultError
    {
        public ResultError()
        {
        }

        public ResultError(string code)
        {
            Code = code;
        }

        public static ResultError FromErrorCode (string errorCode)
            => new ResultError(errorCode);

        public string Code { get; set; }
    }
}