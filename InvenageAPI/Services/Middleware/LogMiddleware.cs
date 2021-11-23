using InvenageAPI.Services.Extension;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Middleware
{
    // Ref: https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/
    public class LogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await LogRequest(httpContext.Request, httpContext.Connection.RemoteIpAddress.ToString());

            var originalBodyStream = httpContext.Response.Body;
            using var responseBody = new MemoryStream();
            httpContext.Response.Body = responseBody;

            await _next(httpContext);

            await LogResponse(httpContext.Response);

            await responseBody.CopyToAsync(originalBodyStream);
        }

        private async Task LogRequest(HttpRequest request, string ipAddress)
        {
            request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            await request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));

            var bodyAsText = Encoding.UTF8.GetString(buffer);

            request.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation(new { Source = ipAddress, request.Headers, Body = bodyAsText }.ToJson());
        }

        private async Task LogResponse(HttpResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);

            string body = await new StreamReader(response.Body).ReadToEndAsync();

            response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation(new { response.StatusCode, response.Headers, body }.ToJson());
        }
    }
}
