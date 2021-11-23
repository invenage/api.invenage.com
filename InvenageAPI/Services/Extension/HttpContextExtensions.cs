using Microsoft.AspNetCore.Http;

namespace InvenageAPI.Services.Extension
{
    public static class HttpContextExtensions
    {
        public static string GetItem(this HttpContext context, string key)
        {
            if (!context.Items.TryGetValue(key, out var result))
                return null;
            return result?.ToString() ?? "";
        }

        public static bool GetRequestHeader(this HttpContext context, string key, out string result)
        {
            result = context.Request.Headers[key].ToString();
            return !result.IsNullOrEmpty();
        }
    }
}
