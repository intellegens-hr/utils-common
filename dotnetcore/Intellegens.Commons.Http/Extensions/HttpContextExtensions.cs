using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Intellegens.Commons.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Look for ORIGIN and X-ORIGIN headers and return their values
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static (string headerOrigin, string headerXOrigin) GetOriginHeaderValues(this HttpContext httpContext)
        {
            var headers = httpContext
                .Request
                .Headers
                .Select(x => new
                {
                    Key = x.Key.ToLower(),
                    x.Value
                });

            string origin = headers.FirstOrDefault(x => x.Key == "origin")?.Value;
            string xorigin = headers.FirstOrDefault(x => x.Key == "x-origin")?.Value;

            return (origin, xorigin);
        }
    }
}