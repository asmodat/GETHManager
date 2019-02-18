using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using GEthManager.Processing;
using Microsoft.Extensions.Options;

namespace GEthManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly ManagerConfig _cfg;

        public HomeController(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        public string Index() => _cfg.welocomeMessage;
    }

    [Route("api/test")]
    public class TestController : Controller
    {
        private readonly ManagerConfig _cfg;

        public TestController(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        [HttpGet("ping")]
        public string Ping() => "pong";

        [HttpGet("ver")]
        public string Version()
            => $"GETH-MANGER API v{_cfg.version}, ServerTime: {DateTime.UtcNow.ToLongDateTimeString()}";

        /*//Debug Only
        [HttpGet("env")]
        public string Environment()
            => System.Environment.GetEnvironmentVariables().JsonSerialize(Newtonsoft.Json.Formatting.Indented);

        [HttpGet("get")]
        public Task<string> Get([FromQuery]string url)
            => HttpHelper.GET(url, System.Net.HttpStatusCode.OK); //*/
    }
}
