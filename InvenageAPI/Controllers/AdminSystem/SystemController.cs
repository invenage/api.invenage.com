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
using System.Linq;

namespace InvenageAPI.Controllers
{
    /// <summary>
    /// Service for accessing system.
    /// </summary>
    [Route("system")]
    [ApiController]
    [AccessRight(AccessScope.AdminSystem)]
    public class SystemController : ControllerBase
    {
        private readonly IDependent _dependents;
        private readonly ILogger _logger;

        public SystemController(IDependent dependents)
        {
            _dependents = dependents;
            _logger = _dependents.GetLogger<SystemController>();
        }

        /// <summary>
        /// Stop the system.
        /// </summary>
        /// <response code="200">Request accpected.</response>
        [HttpDelete]
        [Route("stop")]
        public ActionResult Stop()
        {
            var cacheSynchronizer = _dependents.GetSynchronizer(SynchronizerType.Cache);
            var storageSynchronizer = _dependents.GetSynchronizer(SynchronizerType.Storage);
            var lifetime = _dependents.GetHostApplicationLifetime();
            TaskExtensions.RunTask(() =>
            {
                lifetime.ApplicationStopping.Register(() =>
                {
                    cacheSynchronizer.Stop();
                    storageSynchronizer.Stop();
                    cacheSynchronizer.Process();
                    storageSynchronizer.Process();
                });
                lifetime.StopApplication();
            });
            return Ok();
        }

        /// <summary>
        /// Get most recently log from the request type storage with option limit records.
        /// </summary>
        /// <param name="type"><see cref="StorageType"/></param>
        /// <param name="limit">No. of records</param>
        /// <response code="200">List of the log records.</response>
        /// <response code="400">Incorrect <see cref="StorageType"/> input.</response>
        /// <response code="404">No log records found.</response>
        [HttpGet]
        [Route("logs/{type}")]
        [Produces("application/json")]
        public ActionResult<LogResponse> Log([FromRoute] string type, [FromQuery] int limit = 100)
        {
            try
            {
                var storage = _dependents.GetStorage(type.PraseAsEnum<StorageType>());
                var data = storage.Get(new QueryModel<LogData>()
                {
                    Database = "Log",
                    Collection = "APILog",
                    Order = x => x.RecordTime,
                    Filter = x => x.RecordTime >= DateTimeOffset.UtcNow.AddDays(-3).ToUnixTimeMilliseconds(),
                    SortDirection = SortDirection.Desc,
                    Limit = limit
                });

                if (data.Any())
                    return Ok(new LogResponse() { Logs = data });

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest();
            }
        }

        /// <summary>
        /// Request the synchronizer of the request type to run again now.
        /// </summary>
        /// <param name="type"><see cref="SynchronizerType"/></param>
        /// <response code="200">Request accpected.</response>
        /// <response code="400">Incorrect <see cref="SynchronizerType"/> input.</response>
        [HttpGet]
        [Route("synchronizers/run/{type}")]
        public ActionResult RunSynchronizer([FromRoute] string type)
        {
            try
            {
                var item = _dependents.GetSynchronizer(type.PraseAsEnum<SynchronizerType>());
                item.Process();
            }
            catch (NotSupportedException)
            {
                return BadRequest();
            }
            return Ok();
        }

        /// <summary>
        /// Stop and start the synchronizer of the request type.
        /// </summary>
        /// <param name="type"><see cref="SynchronizerType"/></param>
        /// <response code="200">Request accpected.</response>
        /// <response code="400">Incorrect <see cref="SynchronizerType"/> input.</response>
        [HttpPost]
        [Route("synchronizers/reset/{type}")]
        public ActionResult ResetSynchronizer([FromRoute] string type)
        {
            try
            {
                var item = _dependents.GetSynchronizer(type.PraseAsEnum<SynchronizerType>());
                item.Stop();
                item.Initiate();
            }
            catch (NotSupportedException)
            {
                return BadRequest();
            }
            return Ok();
        }

        /// <summary>
        /// Clear all the key for both local cache and remote cache.
        /// </summary>
        /// <response code="200">Request accpected.</response>
        /// <response code="400">Server error.</response>
        [HttpDelete]
        [Route("caches/clear")]
        public ActionResult ClearCache()
        {
            try
            {
                var cache = _dependents.GetCache();
                cache.Clear(GlobalVariable.CurrentTime, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
                return BadRequest();
            }
            return Ok();
        }
    }
}
