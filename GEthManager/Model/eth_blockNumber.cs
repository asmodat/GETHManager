using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;

namespace GEthManager.Model
{
    public static class eth_blockNumberEx
    {
        public static long TryGetBlockNumber(this eth_blockNumber ebn)
        {
            try
            {
                return ebn.GetBlockNumber();
            }
            catch
            {
                Console.WriteLine($"Failed to convert json rpc (eth_blockNumber) result '{ebn?.result ?? "undefined"}' into block number");
                return -1;
            }
        }
    }

    /// <summary>
    /// example: {"jsonrpc":"2.0","id":83,"result":"0x6e6abc"}
    /// </summary>
    public class eth_blockNumber
    {
        public DateTime TimeStamp { get; set; }

        public string jsonrpc { get; set; }
        public long? id { get; set; }
        public string result { get; set; }

        public long GetBlockNumber()
            => result.HexToLong();
    }
}
