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

        [HttpGet("EtherscanHeight")]
        public Task<long> Height() => _bsm.TryFetchEtherscanBlockHeight();
        

        /*//Debug Only
        [HttpGet("env")]
        public string Environment()
            => System.Environment.GetEnvironmentVariables().JsonSerialize(Newtonsoft.Json.Formatting.Indented);

        [HttpGet("get")]
        public Task<string> Get([FromQuery]string url)
            => HttpHelper.GET(url, System.Net.HttpStatusCode.OK); //*/
    }
}
