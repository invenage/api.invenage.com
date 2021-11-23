using InvenageAPI.Models;
using InvenageAPI.Services.Attributes;
using InvenageAPI.Services.Dependent;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskExtensions = InvenageAPI.Services.Extension.TaskExtensions;

namespace InvenageAPI.Services.Filter
{
    public class AuthFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger _logger;
        private readonly IDependent _dependent;
        public AuthFilter(ILogger<AuthFilter> logger, IDependent dependent)
        {
            _logger = logger;
            _dependent = dependent;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (GlobalVariable.IsLocal)
            {
                context.HttpContext.Items.Add("userId", "LocalBot");
                return;
            }

            var scope = context.ActionDescriptor.EndpointMetadata.OfType<AccessRightAttribute>().LastOrDefault()?.Scope ?? "";
            _logger.LogDebug($"scope: {scope}");

            if (scope.IsNullOrEmpty())
                return;

            if (!context.HttpContext.GetRequestHeader("Authorization", out var authKey))
            {
                SetUnauthorized(context, "Required Authorization Header");
                return;
            }
            _logger.LogDebug($"authKey: {authKey}");

            context.HttpContext.GetRequestHeader("Client_Id", out var clientId);

            var response = await CheckAccess(scope, authKey, clientId);
            _logger.LogDebug($"response: {response.ToJson()}");
            if (!response.CanAccess)
            {
                SetUnauthorized(context, $"Required Authorization of Scope: \"{scope}\" ");
                return;
            }

            context.HttpContext.Items.Add("scope", scope);
            context.HttpContext.Items.Add("userId", response.UserId);
        }

        private async Task<CheckAccessResponse> CheckAccess(string scope, string authKey, string clientId)
        {
            var resp = new CheckAccessResponse();
            authKey = authKey.Replace("Bearer ", "");
            var cache = _dependent.GetCache();
            var connection = _dependent.GetConnection(ConnectionType.API);

            var cacheKey = $"auth_{authKey.ComputeHash()}";
            try
            {
                if (!cache.Get<AuthDetailsModel>(cacheKey, out var result))
                {
                    // No result found in cache, get from auth server
                    var response = await connection.SendRequestAsync<AuthTokenModel, AuthDetailsModel>("Auth", "check", GlobalVariable.Post, new() { Token = authKey, ClientId = clientId });
                    result = response.Response ?? new();
                    cache.Set(cacheKey, result);
                }
                resp.CanAccess = result?.Scopes?.Any(x => x == scope) ?? false;
                resp.UserId = result?.UserId ?? "";
                if (!result.TokenName.IsNullOrEmpty())
                    TaskExtensions.RunTask(async () => await UpdateLastAccessTime(resp.UserId, result.TokenName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
            return resp;
        }

        private async Task UpdateLastAccessTime(string userId, string tokenName)
        {
            var storage = _dependent.GetStorage();
            var data = (await storage.GetAsync(new QueryModel<TokenData>()
            {
                Database = "Access",
                Collection = "Token",
                Filter = x => x.UserId == userId && x.TokenName == tokenName
            })).FirstOrDefault();

            if (data != null)
            {
                data.LastAccessTime = GlobalVariable.CurrentTime;
                await storage.SaveAsync("Access", "Token", data);
            }
        }

        private static void SetUnauthorized(AuthorizationFilterContext context, string message)
            => context.Result = new UnauthorizedObjectResult(message);

        private class CheckAccessResponse
        {
            public bool CanAccess { get; set; }
            public string UserId { get; set; }
        }
    }
}
