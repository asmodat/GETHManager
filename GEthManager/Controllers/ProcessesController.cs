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
using AsmodatStandard.Extensions.AspNetCore;
using GEthManager.Ententions;
using Microsoft.AspNetCore.Authorization;

namespace GEthManager.Controllers
{
    [Authorize]
    [Route("api/Processes")]
    public class ProcessesController : Controller
    {
        private readonly ManagerConfig _cfg;

        private ProcessManager _pm;

        public ProcessesController(
            IHttpContextAccessor accessor,
            IHostingEnvironment hostingEnvironment,
            ProcessManager pm,
            IOptions<ManagerConfig> cfg)
        {
            _pm = pm;
            _cfg = cfg.Value;
        }

        [HttpGet("List")]
        public IActionResult List(string name = null)
        {
            var results = _pm.GetRunningProcessesList();

            if(results.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed To Access System Processes List");

            name = name?.Trim();
            if (name.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status200OK, results);

            var query = results.Where(x => x.processName != null && x.processName.ToLower() == name.ToLower());

            if (query.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status500InternalServerError, $"NO processess with name '{name}' were found.");

            return StatusCode(StatusCodes.Status200OK, query);
        }

        [HttpGet("Kill")]
        public IActionResult Kill(string id = null)
        {
            var results = _pm.GetRunningProcessesList();

            if (results.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed To Access System Processes List");

            id = id?.Trim();
            if (id.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status500InternalServerError, $"Proces To Terminate was not defined.");

            var query = results.Where(x => (x.processName != null && x.processName.ToLower() == id.ToLower()) || x.id.ToString() == id);

            if (query.IsNullOrEmpty())
                return StatusCode(StatusCodes.Status500InternalServerError, $"NO processess with name or id '{id}' were found.");

            var processList = query.Select(x => x.id.ToString());
            var success = _pm.TryTerminateProcesses(processList);

            if (success)
                return StatusCode(StatusCodes.Status200OK, $"Success, terminated processes: {processList?.JsonSerialize()}");
            else
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to terminate processes with name or id '{id}'.");

        }
    }
}
