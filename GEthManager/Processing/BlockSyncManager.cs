using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using GEthManager.Model;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace GEthManager.Processing
{
    public class BlockSyncManager
    {
        private readonly ManagerConfig _cfg;
        private eth_blockNumber etherscanBlockNumber;
        private eth_blockNumber infuraBlockNumber;
        private eth_blockNumber publicBlockNumber;
        private eth_blockNumber privateBlockNumber;
        private eth_syncing privateSyncing;


        private List<long> blockTimes;
        private long lastBlockTimesBlockNr;
        private Stopwatch blockTimesStopWatch = new Stopwatch();
        private long averageBlockTime = -1;

        private List<float> syncTimes;
        private Stopwatch syncTimesStopWatch = new Stopwatch();
        private long lastSyncBlock = -1;
        private float averageSyncSpeed = -1;

        private static readonly object _locker = new object();


        public BlockSyncManager(IOptions<ManagerConfig> cfg)
        {
            _cfg = cfg.Value;
            blockTimes = new List<long>();
            blockTimesStopWatch.Start();

            syncTimes = new List<float>();
            syncTimesStopWatch.Start();
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

        public async Task<eth_syncing> FetchGethSyncResponse(string gethConnectionString)
        {
            using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(_cfg.defaultHttpClientTimeout) })
            {
                var content = new StringContent(
                    _cfg.gethBlockSyncFetchContent,
                    encoding: Encoding.UTF8,
                    mediaType: "application/json");

                var response = await client.POST<eth_syncing>(
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
                TryUpdateBlockTimes(response);
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
                TryUpdateBlockTimes(response);
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
                TryUpdateBlockTimes(response);
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
                TryUpdateBlockTimes(response);
                return true;
            }

            return false;
        }


        public async Task<bool> TryUpdatePrivateSyncing()
        {
            eth_syncing response;
            try
            {
                var connectionString = _cfg.GetPrivateGethConnectionString();
                response = await FetchGethSyncResponse(connectionString);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to fetch private geth block height.");
                Console.WriteLine(ex.JsonSerializeAsPrettyException());
                return false;
            }

            if (response == null || response.result is bool)
                return false;

            var nr = response.TryGetCurrentBlock();

            if (nr > (privateSyncing?.TryGetCurrentBlock() ?? 0))
            {
                response.name = "private";
                privateSyncing = response;
                TryUpdateBlockTimes(
                    currentBlock: null, 
                    currentBlockOverride: response.TryGetHighestBlock());
                TryUpdateSyncTimes(response);
                return true;
            }

            return false;
        }

        private void TryUpdateSyncTimes(eth_syncing syncing)
        {
            lock (_locker)
            {
                var currentSyncBlockNr = syncing.TryGetCurrentBlock();

                if (lastSyncBlock >= currentSyncBlockNr ||
                    currentSyncBlockNr <= 0)
                    return;

                if(syncTimes.Count == 0 && lastSyncBlock <= 0)
                {
                    syncTimesStopWatch.Restart();
                    lastSyncBlock = currentSyncBlockNr;
                    return;
                }

                syncTimes.Add(((float)(currentSyncBlockNr - lastSyncBlock) / syncTimesStopWatch.ElapsedMilliseconds) * 1000);
                syncTimesStopWatch.Restart();

                if (syncTimes.Count > _cfg.syncTimesAverageCount)
                    syncTimes.RemoveAt(0);

                averageSyncSpeed = (long)syncTimes.Average();
                lastSyncBlock = currentSyncBlockNr;
            }
        }

        private void TryUpdateBlockTimes(eth_blockNumber currentBlock, long? currentBlockOverride = null)
        {
            lock (_locker)
            {
                var currentBlockNr = currentBlockOverride ?? currentBlock.TryGetBlockNumber();

                if (lastBlockTimesBlockNr == currentBlockNr ||
                    currentBlockNr <= 0 ||
                    this.GetLastBlockNr().TryGetBlockNumber() > currentBlockNr)
                    return;

                if (lastBlockTimesBlockNr <= 0 ||
                   currentBlockNr > lastBlockTimesBlockNr + 1)
                {
                    lastBlockTimesBlockNr = currentBlockNr;

                    blockTimesStopWatch.Restart();
                    return;
                }

                blockTimes.Add(blockTimesStopWatch.ElapsedMilliseconds);

                if (blockTimes.Count > _cfg.bockTimesAverageCount)
                    blockTimes.RemoveAt(0);

                averageBlockTime = (long)blockTimes.Average();
                lastBlockTimesBlockNr = currentBlockNr;
                blockTimesStopWatch.Restart();
            }
        }

        public eth_blockNumber GetInfuraBlockNr() => infuraBlockNumber;
        public eth_blockNumber GetEtherScanBlockNr() => etherscanBlockNumber;
        public eth_blockNumber GetPublicBlockNr() => publicBlockNumber;
        public eth_blockNumber GetPrivateBlockNr() => privateBlockNumber;
        public eth_syncing GetPrivateSyncing() => privateSyncing;

        public float GetAverageBlockTime()
        {
            if (averageBlockTime < 0)
                return -1;

            return (float)averageBlockTime / 1000;
        }
        public float GetAverageSyncSpeed() => averageSyncSpeed;


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
