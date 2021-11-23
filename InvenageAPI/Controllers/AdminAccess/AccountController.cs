using InvenageAPI.Models;
using InvenageAPI.Services.Attributes;
using InvenageAPI.Services.Constant;
using InvenageAPI.Services.Dependent;
using InvenageAPI.Services.Enum;
using InvenageAPI.Services.Extension;
using InvenageAPI.Services.Global;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace InvenageAPI.Controllers
{
    [Route("account")]
    [ApiController]
    [AccessRight(AccessScope.AdminAccess)]
    public class AccountController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public AccountController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<AccountController>();
        }


        [HttpPost]
        [AccessRight(AccessScope.AllowAnonymous)]
        public async Task<ActionResult> CreateAccount([FromBody] SetAccountRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var result = await GetAuthAccountData(request.Alias);
                if (result != null)
                    return BadRequest("Already have record with same alias.");
                var data = new AccountData()
                {
                    Alias = request.Alias,
                    Passcode = request.Passcode.ComputeSaltedHash(type: HashType.SHA512).ComputeSaltedHash(),
                    UserId = Guid.NewGuid().ToString()
                };
                await storage.SaveAsync("Auth", "Account", data);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        [HttpPatch]
        public async Task<ActionResult> UpdateAccount([FromBody] SetAccountRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");
                var result = await GetAuthAccountData(request.Alias, userId);
                if (result == null)
                    return BadRequest("Cannot found the record for input alias.");
                result.Passcode = request.Passcode.ComputeSaltedHash(type: HashType.SHA512).ComputeSaltedHash();
                await storage.SaveAsync("Auth", "Account", result);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");
                var result = await GetAuthAccountData(request.Alias, userId);
                if (result == null)
                    return BadRequest("Cannot found the record for input alias.");
                await storage.DelectAsync("Auth", "Account", result.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        private async Task<AccountData> GetAuthAccountData(string alias, string userId = null)
        {
            var query = GetAccountQueryModel();
            if (userId == null)
                query.Filter = x => x.Alias == alias;
            else
                query.Filter = x => x.Alias == alias && x.UserId == userId;
            return (await _dependents.GetStorage().GetAsync(query)).FirstOrDefault();
        }

        private static QueryModel<AccountData> GetAccountQueryModel()
            => new() { Database = "Auth", Collection = "Account" };
    }
}
