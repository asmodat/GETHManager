using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;

namespace GEthManager.Processing
{
    public class BlockSyncManager
    {
        private readonly ManagerConfig _cfg;
        private etherscanBlockNrResponse etherscanResponse;

        public BlockSyncManager(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
        }

        /// <summary>
        /// EtherScan Rate limit is 5 requests per second
        /// </summary>
        /// <returns></returns>
        public async Task<etherscanBlockNrResponse> FetchEtherscanBlockResponse()
        {
            if (etherscanResponse != null && (DateTime.UtcNow - etherscanResponse.TimeStamp).TotalSeconds < _cfg.apiRequestRateLimi)
                return etherscanResponse;

            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var requestUri = _cfg.GetEtherscanConnectionString() + _cfg.etherscanBlockHeightFetchQuery;
                var response = await client.GET<etherscanBlockNrResponse>(
                    requestUri: requestUri, 
                    ensureStatusCode: System.Net.HttpStatusCode.OK);

                response.TimeStamp = DateTime.UtcNow;
                return response;
            }
        }

        public async Task<long> TryFetchEtherscanBlockHeight()
        {
            etherscanBlockNrResponse response;
            try
            {
                response = await FetchEtherscanBlockResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch etherscan block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return -1;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > 0)
                etherscanResponse = response;

            return nr;
        }

        /// <summary>
        /// Infure has no Rate limit, but requests should be rate limited to up 10 requests per second
        /// </summary>
        /// <returns></returns>
        public async Task<etherscanBlockNrResponse> FetchInfuraBlockResponse()
        {
            if (etherscanResponse != null && (DateTime.UtcNow - etherscanResponse.TimeStamp).TotalSeconds < _cfg.apiRequestRateLimi)
                return etherscanResponse;

            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var requestUri = _cfg.GetInfuraConnectionString() + _cfg.infuraBlockHeightFetchQuery;
                var response = await client.GET(
                    requestUri: requestUri,
                    ensureStatusCode: System.Net.HttpStatusCode.OK);

                //response.TimeStamp = DateTime.UtcNow;
                //return response;

                return null;
            }
        }

        public async Task<long> TryFetchInfuraBlockHeight()
        {
            etherscanBlockNrResponse response;
            try
            {
                response = await FetchInfuraBlockResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch etherscan block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return -1;
            }

            var nr = response.TryGetBlockNumber();

            if (nr > 0)
                etherscanResponse = response;

            return nr;
        }
    }
}
