using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Intellegens.Commons.Rest
{
    /// <summary>
    /// Simple REST client with error handling and serialization/deserialization
    /// </summary>
    public class RestClient : IDisposable
    {
        protected HttpClient httpClient;

        /// <summary>
        /// Sets given httpClient for all API calls.
        /// </summary>
        /// <param name="httpClient"></param>
        public RestClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Default constructor, initializes new HttpClient which will be used by all API calls
        /// </summary>
        public RestClient()
        {
            this.httpClient = new HttpClient();
        }

        /// <summary>
        /// Build HttpRequestMessage for given method and URL.
        /// Can be overriden to implement custom logic when building request message
        /// </summary>
        /// <param name="method"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        protected virtual async Task<HttpRequestMessage> GetHttpRequestMessage(HttpMethod method, string url)
        {
            var message = new HttpRequestMessage(method, url);
            return message;
        }

        /// <summary>
        /// Build RestResult from HttpResponseMessage
        /// </summary>
        /// <typeparam name="T">Type to which content should be deserialized</typeparam>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        private async Task<RestResult<T>> GetResponseResult<T>(HttpResponseMessage responseMessage)
        {
            // read raw content from body
            string content = await responseMessage.Content.ReadAsStringAsync();

            var result = new RestResult<T>()
            {
                StatusCode = (int)responseMessage.StatusCode,
                ResponseDataRaw = content
            };

            if (result.Success)
            {
                // Even if API call was successful - it can still contain invalid JSON
                try
                {
                    result.ResponseData = JsonConvert.DeserializeObject<T>(content);
                }
                catch (Exception ex)
                {
                    result.StatusCode = 0;
                    result.ErrorMessages.Add("Rest client - error parsing JSON: " + ex.Message);
                }
            }
            else
            {
                string errorMessage = responseMessage.StatusCode.ToString();
                if (!string.IsNullOrEmpty(content))
                    errorMessage = $"{errorMessage}: {content}";

                result.ErrorMessages.Add(errorMessage);
            }

            return result;
        }

        /// <summary>
        /// Call given url using GET method
        /// </summary>
        /// <typeparam name="T">Type to which content should be deserialized</typeparam>
        /// <param name="url">Url to call</param>
        /// <returns></returns>
        public virtual async Task<RestResult<T>> Get<T>(string url)
        {
            var message = await GetHttpRequestMessage(HttpMethod.Get, url);
            var response = await httpClient.SendAsync(message);

            return await GetResponseResult<T>(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">Type to which content should be deserialized</typeparam>
        /// <param name="url">Url to call</param>
        /// <returns></returns>
        public virtual async Task<RestResult<T>> Delete<T>(string url)
        {
            var message = await GetHttpRequestMessage(HttpMethod.Delete, url);
            var response = await httpClient.SendAsync(message);

            return await GetResponseResult<T>(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">Type to which content should be deserialized</typeparam>
        /// <param name="url">Url to call</param>
        /// <param name="dataToSend">Data to serialize and send as application/json</param>
        /// <returns></returns>
        public virtual async Task<RestResult<T>> Post<T>(string url, object dataToSend = null)
        {
            var message = await GetHttpRequestMessage(HttpMethod.Post, url);

            if (dataToSend != null)
                message.Content = new StringContent(JsonConvert.SerializeObject(dataToSend), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(message);

            return await GetResponseResult<T>(response);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T">Type to which content should be deserialized</typeparam>
        /// <param name="url">Url to call</param>
        /// <param name="dataToSend">Data to serialize and send as application/json</param>
        /// <returns></returns>
        public virtual async Task<RestResult<T>> Put<T>(string url, object dataToSend = null)
        {
            var message = await GetHttpRequestMessage(HttpMethod.Put, url);

            if (dataToSend != null)
                message.Content = new StringContent(JsonConvert.SerializeObject(dataToSend), Encoding.UTF8, "application/json");

            var response = await httpClient.SendAsync(message);

            return await GetResponseResult<T>(response);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}