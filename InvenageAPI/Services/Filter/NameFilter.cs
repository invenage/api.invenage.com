using InvenageAPI.Models;
using InvenageAPI.Services.Dependent;
using InvenageAPI.Services.Extension;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvenageAPI.Services.Filter
{
    public class NameFilter : IAsyncActionFilter
    {
        private readonly ILogger _logger;
        private readonly IDependent _dependent;
        public NameFilter(ILogger<NameFilter> logger, IDependent dependent)
        {
            _logger = logger;
            _dependent = dependent;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // If no user id or scope (i.e. allow anonymous) then no need to apply name filter
            if (!context.HttpContext.GetRequestHeader("userId", out var userId) ||
                !context.HttpContext.GetRequestHeader("scope", out var scope))
            {
                await next();
                return;
            }
            _logger.LogDebug($"userId: {userId}, scope: {scope}");
            var cache = _dependent.GetCache();
            var storage = _dependent.GetStorage();

            var cacheKey = $"name_{userId}{scope}";
            var name = "";
            try
            {
                if (!cache.Get(cacheKey, out name))
                {
                    var result = (await storage.GetAsync(new QueryModel<NameData> { Database = "Access", Collection = "Name", Filter = x => x.UserId == userId && x.Scope == scope }))
                        .FirstOrDefault();
                    name = result == null ? userId.ComputeSaltedHash()
                        : result.UseSaltedName ? userId.ComputeSaltedHash(result.Salt) : result.DisplayName;
                    cache.Set(cacheKey, name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                throw;
            }
            context.HttpContext.Items.Add("name", name);
            _logger.LogDebug($"name: {name}");
            await next();
        }
    }
}
