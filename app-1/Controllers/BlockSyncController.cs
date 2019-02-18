using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using GEthManager.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace GEthManager.Controllers
{
    [Route("api/BlockSync")]
    public class BlockSyncController : Controller
    {
        private readonly ManagerConfig _cfg;

        private BlockSyncManager _bsm;

        public BlockSyncController(
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

        [HttpGet("EtherScanHeight")]
        public Task<long> EtherScanHeight() => _bsm.TryFetchEtherscanBlockHeight();

        [HttpGet("InfuraHeight")]
        public Task<long> InfuraHeight() => _bsm.TryFetchInfuraBlockHeight();


        /*//Debug Only
        [HttpGet("env")]
        public string Environment()
            => System.Environment.GetEnvironmentVariables().JsonSerialize(Newtonsoft.Json.Formatting.Indented);

        [HttpGet("get")]
        public Task<string> Get([FromQuery]string url)
            => HttpHelper.GET(url, System.Net.HttpStatusCode.OK); //*/
    }
}
