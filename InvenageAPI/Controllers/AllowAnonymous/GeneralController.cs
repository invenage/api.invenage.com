using Microsoft.AspNetCore.Mvc;
using System;

namespace InvenageAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class GeneralController : ControllerBase
    {
        /// <summary>
        /// System health check.
        /// </summary>
        /// <response code="200">System is normal.</response>
        [HttpGet]
        [Route("health")]
        public ActionResult GetHealth()
        {
            return Ok();
        }

        /// <summary>
        /// System health check.
        /// </summary>
        /// <response code="200">System is normal.</response>
        [HttpPost]
        [Route("health")]
        public ActionResult PostHealth()
        {
            return Ok();
        }
    }
}
