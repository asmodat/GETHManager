using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AsmodatStandard.Extensions;

namespace GEthManager.Model
{
    /// <summary>
    /// example: {"jsonrpc":"2.0","id":83,"result":"0x6e6abc"}
    /// </summary>
    public class etherscanBlockNrResponse
    {
        public DateTime TimeStamp { get; set; }

        public string jsonrpc { get; set; }
        public long? id { get; set; }
        public string result { get; set; }

        public long GetBlockNumber()
            => result.HexToLong();

        public long TryGetBlockNumber()
        {
            try
            {
                return GetBlockNumber();
            }
            catch
            {
                Console.WriteLine($"Failed to convert etherscanBlockNrResponse result '{result ?? "undefined"}' into block number");
                return -1;
            }
        }

    }
}
