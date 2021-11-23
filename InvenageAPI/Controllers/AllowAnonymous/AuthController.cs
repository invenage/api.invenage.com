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
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public AuthController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<AuthController>();
        }

        [HttpPost]
        [Route("login")]
        [Produces("application/json")]
        public async Task<ActionResult<AuthTokenData>> Login([FromBody] SetAccountRequest request)
        {
            try
            {
                var result = await GetAuthData(request.Alias);
                if (result == null || request.Passcode.ComputeSaltedHash(type: HashType.SHA512).ComputeSaltedHash() != result.Passcode)
                    return BadRequest("Invalid Alias Passcode pair.");

                return await CreateToken(new()
                {
                    GrantType = "token",
                    UserId = result.UserId,
                    ClientId = GlobalVariable.ClientId,
                    ClientSecret = GlobalVariable.ClientSecret,
                    Scopes = AccessScope.GetScopesList().ToList(),
                    TokenName = "default"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        [HttpPost]
        [Route("auth")]
        [Produces("application/json")]
        public async Task<ActionResult<AuthTokenData>> CreateToken([FromBody] AuthDetailsModel request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                // TODO: change request model
                var query = GetAccountQueryModel();
                query.Filter = x => x.UserId == request.UserId;
                var user = (await storage.GetAsync(query)).FirstOrDefault();
                if (user == null)
                    return Unauthorized();

                // TODO: Generate Token

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        [HttpPost]
        [Route("check")]
        [Produces("application/json")]
        public async Task<ActionResult<AuthTokenData>> CheckToken([FromBody] AuthTokenModel request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var query = GetAuthTokeQuerynModel();
                query.Filter = x => x.Token == request.Token && x.ClientId == request.ClientId;

                var result = (await storage.GetAsync(query)).FirstOrDefault();
                if (result == null)
                    return Unauthorized();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        private async Task<AccountData> GetAuthData(string alias, string userId = null)
        {
            var query = GetAccountQueryModel();
            if(userId == null)
                query.Filter = x => x.Alias == alias;
            else
                query.Filter = x => x.Alias == alias && x.UserId == userId;
            return (await _dependents.GetStorage().GetAsync(query)).FirstOrDefault();
        }

        private static QueryModel<AccountData> GetAccountQueryModel()
            => new() { Database = "Auth", Collection = "Account" };

        private static QueryModel<AuthTokenData> GetAuthTokeQuerynModel()
            => new() { Database = "Auth", Collection = "Token" };
    }
}
