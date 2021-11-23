using InvenageAPI.Models;
using InvenageAPI.Services.Dependent;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Middleware
{
    public class CORSMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IDependent _dependent;

        public CORSMiddleware(RequestDelegate next, ILogger<CORSMiddleware> logger, IDependent dependent)
        {
            _next = next;
            _logger = logger;
            _dependent = dependent;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (await CheckCors(httpContext))
                await _next(httpContext);
        }

        private async Task<bool> CheckCors(HttpContext httpContext)
        {
            if (GlobalVariable.IsLocal ||
                !httpContext.GetRequestHeader("Origin", out var origin))
                return true;

            if (!httpContext.GetRequestHeader("client_id", out var clientId) ||
                !httpContext.GetRequestHeader("apiKey", out var apiKey) ||
                !await CheckAccess(clientId, origin, apiKey))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                return false;
            }

            AddCORSHeaders(httpContext.Request, httpContext.Response);

            if (httpContext.Request.Method == "OPTIONS")
            {
                httpContext.Response.StatusCode = StatusCodes.Status204NoContent;
                return false;
            }
            return true;
        }

        private async Task<bool> CheckAccess(string clientId, string origin, string apiKey)
        {
            var cache = _dependent.GetCache();
            var storage = _dependent.GetStorage();

            var cacheKey = $"cors_{(clientId + origin + apiKey).ComputeHash()}";

            bool canAccess;
            try
            {
                if (!cache.Get<bool>(cacheKey, out var result))
                {
                    // No result found in cache, search database
                    var record = await storage.GetAsync<CorsData>(new() { Database = "Access", Collection = "CORS", Filter = x => x.Origin == origin && x.ClientId == clientId });
                    result = record.Any() && record.Any(x => x.ApiKey == apiKey);
                    cache.Set(cacheKey, result);
                }
                canAccess = result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                canAccess = false;
            }

            _logger.LogDebug($"CORS canAccess: {canAccess}");
            return canAccess;
        }

        private static void AddCORSHeaders(HttpRequest request, HttpResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", request.Headers["Origin"]);
            response.Headers.Add("Access-Control-Request-Methods", new StringValues("*"));
            response.Headers.Add("Access-Control-Request-Headers", new StringValues("*"));
            response.Headers.Add("Access-Control-Allow-Credentials", new StringValues("true"));
            response.Headers.Add("Access-Control-Max-Age", new StringValues("86400"));
        }
    }
}
