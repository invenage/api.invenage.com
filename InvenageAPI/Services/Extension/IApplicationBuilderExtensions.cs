using InvenageAPI.Services.Middleware;
using Microsoft.AspNetCore.Builder;

namespace InvenageAPI.Services.Extension
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogMiddleware>();
        }
        public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
        public static IApplicationBuilder UseCORSMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CORSMiddleware>();
        }
    }
}
