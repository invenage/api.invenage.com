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
    [Route("name")]
    [ApiController]
    [AccessRight(AccessScope.AdminAccess)]
    public class NameController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public NameController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<NameController>();
        }


        /// <summary>
        /// Get the display value for the username of the specific scope.
        /// </summary>
        /// <response code="200">The list of the data.</response>
        /// <response code="400">The operation failed.</response>
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<NameData>>> GetNameDisplay()
        {
            var storage = _dependents.GetStorage();
            try
            {
                var userId = HttpContext.GetItem("userId");

                var query = GetNameQueryModel();
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
        /// Change the display value for the username of the specific scope.
        /// </summary>
        /// <param name="request">The scope and the salt value to be inserted or updated.</param>
        /// <response code="200">The new name display for the user within specific scope.</response>
        /// <response code="400">The operation failed.</response>
        [HttpPatch]
        [Produces("application/json")]
        public async Task<ActionResult<ChangeNameResponse>> UpdateNameDisplay([FromBody] ChangeNameRequest request)
        {
            var storage = _dependents.GetStorage();
            try
            {
                if (!AccessScope.IsVaildScope(request.Scope))
                    return BadRequest("Invalid Scope Request value.");

                var userId = HttpContext.GetItem("userId");

                var query = GetNameQueryModel();
                query.Filter = x => x.UserId == userId && x.Scope == request.Scope;
                var model = (await storage.GetAsync(query)).FirstOrDefault() ??
                    new NameData()
                    {
                        UserId = userId,
                        Scope = request.Scope,
                    };

                model.Salt = request.Salt ?? model.Salt;
                model.UseSaltedName = request.UseSaltedName ?? model.UseSaltedName;
                model.DisplayName = request.DisplayName ?? model.DisplayName;
                model.RecordTime = GlobalVariable.CurrentTime;
                await storage.SaveAsync("Access", "Name", model);

                return Ok(new ChangeNameResponse()
                {
                    Scope = model.Scope,
                    NewName = model.UseSaltedName ? model.UserId.ComputeSaltedHash(model.Salt) : model.DisplayName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest(GlobalVariable.ErrorMessage);
            }
        }

        private static QueryModel<NameData> GetNameQueryModel()
            => new() { Database = "Access", Collection = "Name" };
    }
}
