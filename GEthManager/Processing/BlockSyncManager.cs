using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;
using System.Text;
using System.Linq;

namespace GEthManager.Processing
{
    public class BlockSyncManager
    {
        private readonly ManagerConfig _cfg;
        private eth_blockNumber etherscanBlockNumber;
        private eth_blockNumber infuraBlockNumber;
        private eth_blockNumber publicBlockNumber;
        private eth_blockNumber privateBlockNumber;


        public BlockSyncManager(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        /// <summary>
        /// EtherScan Rate limit is 5 requests per second
        /// </summary>
        /// <returns></returns>
        public async Task<eth_blockNumber> FetchEtherscanBlockResponse(string requestUri)
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                
                var response = await client.GET<eth_blockNumber>(
                    requestUri: requestUri,
                    ensureStatusCode: System.Net.HttpStatusCode.OK);

                response.TimeStamp = DateTime.UtcNow;
                return response;
            }
        }

        /// <summary>
        /// Infura has no Rate limit, but requests should be rate limited to up 10 requests per second
        /// </summary>
        /// <returns></returns>
        public async Task<eth_blockNumber> FetchInfuraBlockResponse(string requestUri)
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                

                var content = new StringContent(
                    _cfg.infuraBlockHeightFetchContent,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json");

                var response = await client.POST<eth_blockNumber>(
                    requestUri: requestUri, content: content,
                    ensureStatusCode: System.Net.HttpStatusCode.OK);

                response.TimeStamp = DateTime.UtcNow;
                return response;
            }
        }

        public async Task<eth_blockNumber> FetchGethBlockResponse(string gethConnectionString)
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var content = new StringContent(
                    _cfg.gethBlockHeightFetchContent,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json");

                var response = await client.POST<eth_blockNumber>(
                    requestUri: gethConnectionString, content: content,
                    ensureStatusCode: System.Net.HttpStatusCode.OK);

                response.TimeStamp = DateTime.UtcNow;
                return response;
            }
        }

        public async Task<bool> TryUpdateInfuraBlockHeight()
        {
            eth_blockNumber response;
            try
            {
                var requestUri = _cfg.GetInfuraConnectionString() + _cfg.infuraBlockHeightFetchQuery;
                response = await FetchInfuraBlockResponse(requestUri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch infura block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return false;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > (infuraBlockNumber?.TryGetBlockNumber() ?? 0))
            {
                response.name = "infura";
                infuraBlockNumber = response;
                return true;
            }

            return false;
        }
        public async Task<bool> TryUpdateEtherscanBlockHeight()
        {
            eth_blockNumber response;
            try
            {
                var requestUri = _cfg.GetEtherscanConnectionString() + _cfg.etherscanBlockHeightFetchQuery;
                response = await FetchEtherscanBlockResponse(requestUri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch etherscan block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return false;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > (etherscanBlockNumber?.TryGetBlockNumber() ?? 0))
            {
                response.name = "etherscan";
                etherscanBlockNumber = response;
                return true;
            }

            return false;
        }
        public async Task<bool> TryUpdatePublicBlockHeight()
        {
            eth_blockNumber response;
            try
            {
                var connectionString = _cfg.GetPublicGethConnectionString();
                response = await FetchGethBlockResponse(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch public geth block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return false;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > (publicBlockNumber?.TryGetBlockNumber() ?? 0))
            {
                response.name = "public";
                publicBlockNumber = response;
                return true;
            }

            return false;
        }
        public async Task<bool> TryUpdatePrivateBlockHeight()
        {
            eth_blockNumber response;
            try
            {
                var connectionString = _cfg.GetPrivateGethConnectionString();
                response = await FetchGethBlockResponse(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch private geth block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return false;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > (privateBlockNumber?.TryGetBlockNumber() ?? 0))
            {
                response.name = "private";
                privateBlockNumber = response;
                return true;
            }

            return false;
        }

        public eth_blockNumber GetInfuraBlockNr() => infuraBlockNumber;
        public eth_blockNumber GetEtherScanBlockNr() => etherscanBlockNumber;
        public eth_blockNumber GetPublicBlockNr() => publicBlockNumber;
        public eth_blockNumber GetPrivateBlockNr() => privateBlockNumber;


        public eth_blockNumber[] GetAllBlocksNr(bool apiOnly = false) => apiOnly ? new eth_blockNumber[] {
                infuraBlockNumber,
                etherscanBlockNumber,
                publicBlockNumber,
                privateBlockNumber
            } : new eth_blockNumber[] {
                infuraBlockNumber,
                etherscanBlockNumber,
            };

        /// <summary>
        /// Returns Last Detected eth_blockNumber
        /// </summary>
        /// <returns>null or eth_blockNumber object</returns>
        public eth_blockNumber GetLastBlockNr(bool apiOnly = false)
        {
            var blocks = GetAllBlocksNr(apiOnly: apiOnly);

            eth_blockNumber max = null;
            foreach(var block in blocks)
            {
                if (block == null)
                    continue;

                if(max == null)
                {
                    max = block;
                    continue;
                }

                if (max.blockNumber < block.blockNumber)
                {
                    max = block;
                    continue;
                }

                //The most early detected block timestamp is used to keep consistent results of the API
                if (max.blockNumber == block.blockNumber)
                {
                    if (max.TimeStamp > block.TimeStamp)
                        max = block;
                }
            }

            return max;
        }
    }
}
