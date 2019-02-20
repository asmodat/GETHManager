using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using GEthManager.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using GEthManager.Model;

namespace GEthManager.Controllers
{
    [Route("api/Resources")]
    public class ResourcesController : Controller
    {
        private readonly ManagerConfig _cfg;

        private BlockSyncManager _bsm;

        public ResourcesController(
            IHttpContextAccessor accessor,
            IHostingEnvironment hostingEnvironment,
            BlockSyncManager bsm,
            IOptions<ManagerConfig> cfg)
        {
            _bsm = bsm;
            _cfg = cfg.Value;
        }


        [HttpGet("ping")]
        public string Ping() => "pong";
    }
}
