using System.Collections.Generic;

namespace Intellegens.Commons.Rest
{
    /// <summary>
    /// API REST result class
    /// Each API call should at least return status code and error messages (if any)
    /// </summary>
    public class RestResult
    {
        public RestResult()
        {
        }

        public bool Success => StatusCode == 200;
        public int StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }

    /// <summary>
    /// API result with payload
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RestResult<T> : RestResult
    {
        public RestResult() : base()
        {
        }

        public RestResult(string rawData, T data) : base()
        {
            ResponseDataRaw = rawData;
            ResponseData = data;
        }

        /// <summary>
        /// Raw data - this should be set even if JSON deserialization failed
        /// </summary>
        public string ResponseDataRaw { get; set; }

        /// <summary>
        /// Deserialized ResponseDataRaw property
        /// </summary>
        public T ResponseData { get; set; }
    }
}