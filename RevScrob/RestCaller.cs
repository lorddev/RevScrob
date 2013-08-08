using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RevScrob
{
    class RestCaller
    { 
        public RestCaller()
        {
            _queryParams = new Dictionary<string, string>();
            Method = "GET";
        }

        public string Host { get; set; }

        public string Method { get; set; }

        // For Last.fm use "/2.0/"
        public string Action { get; set; }

        public object Payload { get; set; }

        private IDictionary<string, string> _queryParams;

        public RestCaller AddParam(string key, string value)
        {
            _queryParams.Add(key, value);
            return this;
        }

        public async Task<IApiResult> ExecuteAsync()
        {
            return await ExecuteAsync<dynamic>();
        }

        public interface IApiResult
        {
            dynamic Data { get; set; }
        }

        public struct ApiResult : IApiResult
        {
            public dynamic Data { get; set; }
            public HttpStatusCode StatusCode { get; set; }
            public dynamic Error { get; set; }
        }

        public async Task<IApiResult> ExecuteAsync<T>() where T : class
        {
            HttpResponseMessage response = await CallService(JsonConvert.SerializeObject(Payload));
        
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return new ApiResult { StatusCode = response.StatusCode };
            }

            if (response.Content != null)
            {
                var task = response.Content.ReadAsStringAsync();
                var content = task.Result;

                // Assume we know how to deserialize the object.
                dynamic error = null;
                dynamic dataItem = null;// = new T[0];

                if (content.Contains("error"))
                {
                    error = JObject.Parse(content);
                }
                else
                {
                    // try as array
                    try
                    {
                        dataItem = JsonConvert.DeserializeObject<T[]>(content);
                    }
                    catch
                    {
                        dataItem =
                            new[]
                                {
                                    JsonConvert.DeserializeObject(content) as T
                                };
                    }

                    dataItem = JObject.Parse(content);
                }

                return new ApiResult
                {
                    StatusCode = response.StatusCode,
                    Data = dataItem,
                    Error = error
                };

                throw new Exception("Unable to deserialize");
                //return new ApiResult<string>
                //    {
                //        StatusCode = response.StatusCode,
                //        DataItem = content
                //    };
            }

            throw new Exception("Unable to deserialize");
              
            //return new ApiResult { StatusCode = response.StatusCode };
        }

        private CookieContainer _cookies = new CookieContainer();

        private async Task<HttpResponseMessage> CallService(string payload)
        {
            HttpResponseMessage response;
            var endpoint = BuildEndpoint() + BuildQueryString();

            Debug.WriteLine(endpoint);
            var handler = new HttpClientHandler { CookieContainer = _cookies };
         
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.BaseAddress = new Uri(Host);

                // User-Agent: Fiddler Web Debugger
                // Content-Type: application/json; charset=utf-8
                // Content-Length: 0

                switch (Method.ToUpper().Trim())
                {
                    case "PUT":
                        response = await PutMessage(payload, httpClient, endpoint);
                        break;
                        
                    case "POST":
                        response = await PostMessage(payload, httpClient, endpoint);
                        break;
                    
                    case "DELETE":
                        response = await HttpDelete(endpoint, httpClient);
                        break;

                    default:
                        // Assume "GET"
                        response = await httpClient.GetAsync(endpoint);
                        break;
                }
            }

            return response;
        }

        private static async Task<HttpResponseMessage> HttpDelete(string endpoint, HttpClient httpClient)
        {
            var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(endpoint),
                    Method = HttpMethod.Delete,
                };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                });

            var response = await httpClient.SendAsync(request);
            
            return response;
        }

        private static async Task<HttpResponseMessage> PostMessage(
            string content, HttpClient httpClient, string endpoint)
        {
            var stringContent = FormatContent(content);
            var task = await httpClient.PostAsync(endpoint, stringContent);
            return task;
        }

        private static StringContent FormatContent(string content)
        {
            var stringContent = new StringContent(content);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json")
                {
                    CharSet = "utf-8"
                };
            return stringContent;
        }

        private static async Task<HttpResponseMessage> PutMessage(string content, HttpClient httpClient, string endpoint)
        {
            var stringContent = FormatContent(content);
            var task = await httpClient.PutAsync(endpoint, stringContent);
            return task;
        }

        private string BuildEndpoint()
        {
            var sb = new StringBuilder(50);
            //sb.Append(Host);
            //if (!Host.EndsWith("/"))
            //{
            //    sb.Append("/");
            //}

            sb.Append(Action.ToLower());
            
            return sb.ToString();
        }

        private string BuildQueryString()
        {
            var sb = new StringBuilder();

            int length = _queryParams.Count;
            int i = 0;
            foreach (var item in _queryParams)
            {
                sb.AppendFormat("{0}={1}", item.Key, item.Value);

                if (i < length - 1)
                {
                    sb.Append("&");
                    i++;
                }
            }

            string qs2 = sb.ToString();
            return "?" + qs2;
        }
    }
}


