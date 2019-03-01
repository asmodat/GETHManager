using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Threading.Tasks;
using GEthManager.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using GEthManager.Model;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace GEthManager.Controllers
{
    [Authorize]
    [Route("api/Resources")]
    public class ResourcesController : Controller
    {
        private readonly ManagerConfig _cfg;

        private PerformanceManager _pm;

        public ResourcesController(
            IHttpContextAccessor accessor,
            IHostingEnvironment hostingEnvironment,
            PerformanceManager pm,
            IOptions<ManagerConfig> cfg)
        {
            _pm = pm;
            _cfg = cfg.Value;
        }


        [HttpGet("CPU")]
        public IActionResult CPU(int? timeframe = null)
        {
            var results = _pm.GetMedianCpuResults();

            if (timeframe == null)
                return StatusCode(StatusCodes.Status200OK, results);

            var index = _pm.TimeFrameIndexes().FindIndex(x => x == timeframe);

            if(index < 0 || index >= results.Length)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unknown TimeFrame");

            return StatusCode(StatusCodes.Status200OK, results[index]);
        }

        [HttpGet("RAM")]
        public IActionResult RAM(int? timeframe = null)
        {
            var results = _pm.GetMedianRamResults();

            if (timeframe == null)
                return StatusCode(StatusCodes.Status200OK, results);


            var index = _pm.TimeFrameIndexes().FindIndex(x => x == timeframe);

            if (index < 0 || index >= results.Length)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unknown TimeFrame");

            return StatusCode(StatusCodes.Status200OK, results[index]);
        }

        [HttpGet("DISK")]
        public IActionResult DISK(string name = null)
        {
            var results = _pm.GetDriveInfo();

            if (results == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "No Drive Info");


            name = name?.ToLower().ReplaceMany((" ", ""), (":", ""), ("/", ""), ("\\", ""));

            if (name.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status200OK, results);

            var drive = results.FirstOrDefault(x => x.name == name);

            if (drive == null)
                return StatusCode(StatusCodes.Status500InternalServerError, $"Drive '{name}' was not found");

            return StatusCode(StatusCodes.Status200OK, drive);
        }


    }
}
