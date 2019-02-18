using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Threading.Tasks;

namespace GEthManager.Processing
{
    public class ManagerConfig
    {
        public string version { get; set; }

        public string[] etherscanApiKeys { get; set; }
        public string[] infuraApiKeys { get; set; }

        public string etherscanApiKey { get; set; }
        public string infuraApiKey { get; set; }

        public string etherscanConnectionString { get; set; }
        public string infuraConnectionString { get; set; }

        public string etherscanBlockHeightFetchQuery { get; set; }
        public string infuraBlockHeightFetchQuery { get; set; }

        public string welocomeMessage { get; set; } = "Welcome To Geth Manager";

        public int defaultHttpClientTimeout { get; set; } = 5;
        public int apiRequestRateLimi { get; set; } = 1;

        public void RotateApiKeys()
        {
            if (etherscanApiKeys.IsNullOrEmpty())
                etherscanApiKeys = new string[] { etherscanApiKey };

            if (infuraApiKeys.IsNullOrEmpty())
                infuraApiKeys = new string[] { infuraApiKey };

            etherscanApiKey = etherscanApiKeys[RandomEx.Next(0, etherscanApiKeys.Length)];
            infuraApiKey = infuraApiKeys[RandomEx.Next(0, infuraApiKeys.Length)];
        }

        public string GetEtherscanConnectionString()
        {
            RotateApiKeys();
            return etherscanConnectionString?.Replace("${etherscanApiKey}", etherscanApiKey);
        }

        public string GetInfuraConnectionString()
        {
            RotateApiKeys();
            return infuraConnectionString?.Replace("${infuraApiKey}", infuraApiKey);
        }
    }
}
