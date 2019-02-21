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
    [Route("api/Block")]
    public class BlockController : Controller
    {
        private readonly ManagerConfig _cfg;

        private BlockSyncManager _bsm;

        public BlockController(
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

            return StatusCode(StatusCodes.Status200OK, bnr);
        }

        [HttpGet("InfuraHeight")]
        public IActionResult InfuraHeight()
        {
            var bnr = _bsm.GetInfuraBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status200OK, bnr);
        }

        [HttpGet("PublicHeight")]
        public IActionResult PublicHeight()
        {
            var bnr = _bsm.GetPublicBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status200OK, bnr);
        }

        [HttpGet("PrivateHeight")]
        public IActionResult PrivateHeight()
        {
            var bnr = _bsm.GetPrivateBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status200OK, bnr);
        }

        /// <summary>
        /// Last Height
        /// </summary>
        /// <returns></returns>
        [HttpGet("Height")]
        public IActionResult Height()
        {
            var bnr = _bsm.GetLastBlockNr().TryGetBlockNumber();

            if (bnr <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status200OK, bnr);
        }

        [HttpGet("eth_blockNumber")]
        public IActionResult eth_blockNumber()
        {
            var ebn = _bsm.GetLastBlockNr();

            if (ebn == null || ebn.TryGetBlockNumber() <= 0)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return StatusCode(StatusCodes.Status200OK, ebn);
        }

        [HttpGet("Heights")]
        public IActionResult HeightRapport() => StatusCode(200, _bsm.GetAllBlocksNr());
    }
}
