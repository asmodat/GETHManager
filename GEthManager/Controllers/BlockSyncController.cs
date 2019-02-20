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


        [HttpGet("EtherScanHeight")]
        public IActionResult EtherScanHeight()
        {
            var bnr = _bsm.GetEtherScanBlockNr().TryGetBlockNumber();

            if(bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(200, bnr);
        }

        [HttpGet("InfuraHeight")]
        public IActionResult InfuraHeight() {

            var bnr = _bsm.GetInfuraBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(200, bnr);
        }


        [HttpGet("PublicHeight")]
        public IActionResult PublicHeight()
        {

            var bnr = _bsm.GetPublicBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(200, bnr);
        }

        [HttpGet("PrivateHeight")]
        public IActionResult PrivateHeight()
        {

            var bnr = _bsm.GetPrivateBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(200, bnr);
        }

    }
}
