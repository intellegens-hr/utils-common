using System.Collections.Generic;

namespace Intellegens.Commons.Rest
{
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

        public string ResponseDataRaw { get;set;}
        public T ResponseData { get; set; }
    }
}