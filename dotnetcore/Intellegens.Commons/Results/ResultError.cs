namespace Intellegens.Commons.Results
{
    /// <summary>
    /// Used by Result class
    /// </summary>
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

        /// <summary>
        /// Error code, message should be proided by frontend
        /// </summary>
        public string Code { get; set; }
    }
}