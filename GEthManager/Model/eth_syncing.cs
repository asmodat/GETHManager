using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;
using Newtonsoft.Json;

namespace GEthManager.Model
{
    public static class eth_syncingEx
    {
        public static long TryGetCurrentBlock(this eth_syncing es)
        {
            try
            {
                return es.GetCurrentBlock();
            }
            catch
            {
                return -1;
            }
        }

        public static long TryGetHighestBlock(this eth_syncing es)
        {
            try
            {
                return es.GetHighestBlock();
            }
            catch
            {
                return -1;
            }
        }
    }

    /// <summary>
    /// example: {"jsonrpc":"2.0","id":83,"result":"0x6e6abc"}
    /// </summary>
    public class eth_syncing
    {
        public string name { get; set; }

        public DateTime TimeStamp { get; set; }

        public string jsonrpc { get; set; }
        public long? id { get; set; }
        public object result { get; set; }

        public eth_syncingResult GetResult()
        {
            if (result == null || result is bool)
                return null;

            return result.JsonSerialize().JsonDeserialize<eth_syncingResult>();
        }

        public long currentBlockNumber { get => this.TryGetCurrentBlock(); }
        public long highestBlockNumber { get => this.TryGetHighestBlock(); }

        public long GetCurrentBlock() => GetResult().currentBlock.HexToLong();
        public long GetHighestBlock() => GetResult().highestBlock.HexToLong();
        public long GetKnownStates() => GetResult().knownStates.HexToLong();
        public long GetPulledStates() => GetResult().pulledStates.HexToLong();
        public long GetStartingBlock() => GetResult().startingBlock.HexToLong();
    }

    public class eth_syncingResult
    {
        public string currentBlock { get; set; }
        public string highestBlock { get; set; }
        public string knownStates { get; set; }
        public string pulledStates { get; set; }
        public string startingBlock { get; set; }
    }
}
