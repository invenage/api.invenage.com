using InvenageAPI.Services.Extension;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace InvenageAPI.Services.Global
{
    public static class GlobalVariable
    {
        public static bool IsLocal
            => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local";

        public static bool IsDevelopment
            => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        public static bool IsProduction
            => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";

        public static string Salt
            => Environment.GetEnvironmentVariable("Salt") ?? "B035E243-FDA4-47BA-AF29-F361B0931CA4";

        public static string ClientId
            => Environment.GetEnvironmentVariable("ClientId") ?? "";

        public static string ClientSecret
            => Environment.GetEnvironmentVariable("ClientSecret") ?? "";

        public static long CurrentTime
            => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static HttpMethod Get
            => HttpMethod.Get;
        public static HttpMethod Post
            => HttpMethod.Post;
        public static HttpMethod Put
            => HttpMethod.Put;
        public static HttpMethod Delete
            => HttpMethod.Delete;

        public const string ErrorMessage
            = "Service error. Please also provide trace id in header to us when requesting support.";
    }
}
