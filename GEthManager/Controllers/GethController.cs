using Microsoft.AspNetCore.Mvc;
using GEthManager.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace GEthManager.Controllers
{
    [Authorize]
    [Route("api/Geth")]
    public class GethController : Controller
    {
        private readonly ManagerConfig _cfg;

        private ProcessManager _pm;

        public GethController(
            IHttpContextAccessor accessor,
            IHostingEnvironment hostingEnvironment,
            ProcessManager pm,
            IOptions<ManagerConfig> cfg)
        {
            _pm = pm;
            _cfg = cfg.Value;
        }

        [HttpGet("OutputLog")]
        public IActionResult OutputLog(int? length = null)
        {
            if(length == null)
                length = _cfg.maxGethInMemoryOutputLogLength;

            if(length.Value <= 0 || length.Value > _cfg.maxGethInMemoryOutputLogLength)
                return StatusCode(StatusCodes.Status500InternalServerError, $"length parameter is out of <1, {_cfg.maxGethInMemoryOutputLogLength}> range.");

            return StatusCode(StatusCodes.Status200OK, _pm.GetOutputLog(length.Value));
        }

        [HttpGet("ErrorLog")]
        public IActionResult ErrorLog(int? length = null)
        {
            if (length == null)
                length = _cfg.maxGethInMemoryErrorLogLength;

            if (length.Value <= 0 || length.Value > _cfg.maxGethInMemoryErrorLogLength)
                return StatusCode(StatusCodes.Status500InternalServerError, $"n parameter is out of <1, {_cfg.maxGethInMemoryErrorLogLength}> range.");

            return StatusCode(StatusCodes.Status200OK, _pm.GetErrorLog(length.Value));
        }
    }
}
