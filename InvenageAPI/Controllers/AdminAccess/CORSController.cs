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
    [Route("cors")]
    [ApiController]
    public class CORSController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public CORSController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<CORSController>();
        }

        /// <summary>
        /// Get CORS request under the user.
        /// </summary>
        /// <response code="200">The list of the data.</response>
        /// <response code="400">The operation failed.</response>
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<CorsData>>> GetCORSAccess()
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var query = GetCorsQueryModel();
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
        /// Create apiKey for CORS request.
        /// </summary>
        /// <param name="request">The origin and client id that need to access this api.</param>
        /// <response code="200">The apiKey with the allowed origin.</response>
        /// <response code="400">The operation failed.</response>
        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<CORSAccessResponse>> CreateCORSAccess([FromBody] CORSAccessRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var query = GetCorsQueryModel();
                query.Filter = x => x.UserId == userId && x.ClientId == request.ClientId && x.Origin == request.Origin;

                var data = await storage.GetAsync(query);
                if (data.Any())
                    return BadRequest("Already have record with same ClientId and Origin.");

                var model = new CorsData()
                {
                    ClientId = request.ClientId,
                    Origin = request.Origin,
                    UserId = userId
                };
                model.ApiKey = model.ToJson().ComputeSaltedHash();
                await storage.SaveAsync("Access", "CORS", model);

                return Ok(new CORSAccessResponse()
                {
                    ClientId = request.ClientId,
                    Origin = request.Origin,
                    ApiKey = model.ApiKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        /// <summary>
        /// Delete CORS request.
        /// </summary>
        /// <param name="request">The origin and client id record that need to delete.</param>
        /// <response code="200">The apiKey is deleted.</response>
        /// <response code="400">The operation failed.</response>
        [HttpDelete]
        public async Task<ActionResult> DeleteCORSAccess([FromBody] CORSAccessRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var query = GetCorsQueryModel();
                query.Filter = x => x.UserId == userId && x.ClientId == request.ClientId && x.Origin == request.Origin;

                var data = (await storage.GetAsync(query)).FirstOrDefault();

                if (data != null)
                    await storage.DelectAsync("Access", "CORS", data.Id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }
        private static QueryModel<CorsData> GetCorsQueryModel()
            => new() { Database = "Access", Collection = "CORS" };
    }
}
