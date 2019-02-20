using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;
using System.Text;

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
        public async Task<eth_blockNumber> FetchEtherscanBlockResponse()
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var requestUri = _cfg.GetEtherscanConnectionString() + _cfg.etherscanBlockHeightFetchQuery;
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
        public async Task<eth_blockNumber> FetchInfuraBlockResponse()
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var requestUri = _cfg.GetInfuraConnectionString() + _cfg.infuraBlockHeightFetchQuery;

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
                response = await FetchInfuraBlockResponse();
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
                response = await FetchEtherscanBlockResponse();
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
                privateBlockNumber = response;
                return true;
            }

            return false;
        }

        public eth_blockNumber GetInfuraBlockNr() => infuraBlockNumber;
        public eth_blockNumber GetEtherScanBlockNr() => etherscanBlockNumber;
        public eth_blockNumber GetPublicBlockNr() => publicBlockNumber;
        public eth_blockNumber GetPrivateBlockNr() => privateBlockNumber;
    }
}
