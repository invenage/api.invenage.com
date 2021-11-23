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
    [Route("token")]
    [ApiController]
    [AccessRight(AccessScope.AdminAccess)]
    public class TokenController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public TokenController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<TokenController>();
        }

        /// <summary>
        /// Get the tokens created for the user.
        /// </summary>
        /// <response code="200">The list of the data.</response>
        /// <response code="400">The operation failed.</response>
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<TokenData>>> GetToken()
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var query = GetTokenQueryModel();
                query.Filter = x => x.UserId == userId;
                var data = await storage.GetAsync(query);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        /// <summary>
        /// Generate direct access token for the user requested scopes.
        /// </summary>
        /// <param name="request">The token name and the requested scopes.</param>
        /// <response code="200">The token for the user with specific scopes.</response>
        /// <response code="400">The operation failed.</response>
        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<CreateTokenResponse>> CreateToken([FromBody] CreateTokenRequest request)
        {
            var connection = _dependents.GetConnection(ConnectionType.API);
            var storage = _dependents.GetStorage();
            try
            {
                if (!AccessScope.IsVaildScope(request.Scope))
                    return BadRequest("Invalid Scope Request value.");

                var userId = HttpContext.GetItem("userId");

                var query = GetTokenQueryModel();
                query.Filter = x => x.UserId == userId && x.TokenName == request.TokenName;
                var data = await storage.GetAsync(query);
                if (data.Any())
                    return BadRequest("Already have record with same TokenName.");

                var model = new AuthDetailsModel()
                {
                    TokenName = request.TokenName,
                    UserId = userId,
                    Scopes = request.Scope,
                    ClientId = GlobalVariable.ClientId,
                    ClientSecret = GlobalVariable.ClientSecret,
                    GrantType = "token"
                };
                var result = await connection.SendRequestAsync<AuthDetailsModel, AuthTokenModel>("Auth", "auth", GlobalVariable.Post, model) ?? new();
                if (!result.IsSuccess || result.Response.Token.IsNullOrEmpty())
                    throw new Exception("Auth server cannot create token.");
                await storage.SaveAsync("Access", "Token", new TokenData
                {
                    TokenName = request.TokenName,
                    UserId = userId,
                    Scopes = request.Scope
                });
                return Ok(new CreateTokenResponse()
                {
                    Scope = request.Scope,
                    Token = result.Response.Token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        /// <summary>
        /// Delete direct access token for the user.
        /// </summary>
        /// <param name="request">The token name of the token.</param>
        /// <response code="200">The token is deleted.</response>
        /// <response code="400">The operation failed.</response>
        [HttpDelete]
        public async Task<ActionResult> DeleteToken([FromBody] DeleteTokenRequest request)
        {
            var connection = _dependents.GetConnection(ConnectionType.API);
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var model = new AuthDetailsModel()
                {
                    TokenName = request.TokenName,
                    UserId = userId
                };
                var result = await connection.SendRequestAsync<AuthDetailsModel, string>("Auth", "auth", GlobalVariable.Delete, model);
                if (!result.IsSuccess)
                    throw new Exception("Auth server cannot delete token.");

                var data = storage.Get(new QueryModel<TokenData>()
                {
                    Database = "Access",
                    Collection = "Token",
                    Filter = x => x.UserId == userId && x.TokenName == request.TokenName
                }).FirstOrDefault();

                if (data != null)
                    await storage.DelectAsync("Access", "Token", data.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }
        private static QueryModel<TokenData> GetTokenQueryModel()
            => new() { Database = "Access", Collection = "Token" };
    }
}
