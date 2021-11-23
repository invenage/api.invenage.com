using InvenageAPI.Models;
using InvenageAPI.Services.Extension;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Connection
{
    public class ApiConnection : IApiConnection
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public ApiConnection(IConfiguration config, ILogger<ApiConnection> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<APIResponseModel<TResponse>> SendRequestAsync<TRequest, TResponse>(string target, string endPoint, HttpMethod method, TRequest body)
        {
            HttpClient client = new(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
            try
            {
                var model = GetConfigModel(target);
                model.Url += endPoint;
                var request = SetRequestMessage(model, method, body);
                var response = await client.SendAsync(request);
                var responseString = await response?.Content?.ReadAsStringAsync() ?? "";
                var result = new APIResponseModel<TResponse>()
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = response.StatusCode.ToString(),
                    Response = responseString.IsNullOrEmpty() ? default : responseString.FromJson<TResponse>()
                };
                _logger.LogDebug(result.ToJson());
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
            return new APIResponseModel<TResponse>()
            {
                IsSuccess = false,
                StatusCode = null,
                Response = default
            };
        }

        private RequestConfigModel GetConfigModel(string target)
        {
            RequestConfigModel model = new();
            _config.GetSection("Connections").GetSection(target).Bind(model);
            _logger.LogDebug(model.ToJson());
            return model;
        }

        private HttpRequestMessage SetRequestMessage<T>(RequestConfigModel model, HttpMethod method, T body)
        {
            StringContent content = null;
            if (method != HttpMethod.Get)
                content = new StringContent(body.ToJson(), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new()
            {
                Method = method,
                Content = content,
                RequestUri = new Uri(model.Url),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("apiKey", model.APIKey);

            foreach (var header in model.Header)
                request.Headers.Add(header.Key, header.Value);
            _logger.LogDebug(request.ToJson());
            return request;
        }

        private class RequestConfigModel
        {
            public string Url { get; set; }
            public string APIKey { get; set; }
            public Headers[] Header { get; set; }

            public class Headers
            {
                public string Key { get; set; }
                public string Value { get; set; }
            }
        }
    }
}
