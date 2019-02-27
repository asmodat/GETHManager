using Microsoft.AspNetCore.Mvc;
using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using System.Threading.Tasks;
using System.Linq;

namespace GEthManager.Processing
{
    public class ManagerConfig
    {
        public string version { get; set; }
        public string login { get; set; }
        public string password { get; set; }

        public string[] etherscanApiKeys { get; set; }
        public string[] infuraApiKeys { get; set; }

        public string etherscanApiKey { get; set; }
        public string infuraApiKey { get; set; }

        public string etherscanConnectionString { get; set; } = "https://api.etherscan.io/api?apikey=${etherscanApiKey}";
        public string infuraConnectionString { get; set; } = "https://mainnet.infura.io/${infuraApiKey}";
        public string[] publicGEthConnectionStrings { get; set; }
        public string privateGEthConnectionString { get; set; }

        public string etherscanBlockHeightFetchQuery { get; set; } = "&module=proxy&action=eth_blockNumber";
        public string infuraBlockHeightFetchQuery { get; set; } = "";
        public string infuraBlockHeightFetchContent { get; set; } = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"eth_blockNumber\", \"params\": []}";

        public string gethBlockHeightFetchContent { get; set; } = "{\"jsonrpc\": \"2.0\", \"id\": 1, \"method\": \"eth_blockNumber\", \"params\": []}";

        public string welocomeMessage { get; set; } = "Welcome To Geth Manager";

        public int defaultHttpClientTimeout { get; set; } = 5;

        public int etherscanRequestDelay { get; set; } = 1;
        public int infuraRequestDelay { get; set; } = 1;
        public int publicRequestDelay { get; set; } = 1;
        public int privateRequestDelay { get; set; } = 1;

        public int cpuCountSamples { get; set; } = 10000;
        /// <summary>
        /// miliseconds
        /// </summary>
        public int cpuCountIntensity { get; set; } = 500;

        public int ramCountSamples { get; set; } = 10000;
        /// <summary>
        /// miliseconds
        /// </summary>
        public int ramCountIntensity { get; set; } = 500;

        public int diskCheckIntensity { get; set; } = 2500;

        public int processesCheckIntensity { get; set; } = 5000;
        public int gethReStartIntensity { get; set; } = 2500;

        /// <summary>
        /// example: geth - requires to setup PATH varaibles
        /// </summary>
        public string gethStartFileName { get; set; }

        /// <summary>
        /// example: "--syncmode \"fast\" --rpc --rpcapi=\"db,eth,net,web3,personal,txpool\" --cache 2024 --maxpeers 50 --verbosity 3 --rpcport 8545 --rpcaddr \"127.0.0.1\" --rpccorsdomain \"*\" --rpcvhosts \"*\""
        /// </summary>
        public string gethStartArguments { get; set; }
        public int maxGethOutputLogSize { get; set; } = 1 * 1024 * 1024;
        public int maxGethErrorLogSize { get; set; } = 1 * 1024 * 1024;
        public int maxGethInMemoryOutputLogLength { get; set; } = 1 * 1024 * 1024;
        public int maxGethInMemoryErrorLogLength { get; set; } = 1 * 1024 * 1024;

        public string[] gethStartKillProcesses { get; set; }
        public bool enableGethService { get; set; } = true;
        public bool gethEnableConsole { get; set; } = false;
        public bool gethEnableLog { get; set; } = true;
        public string gethOutputLog { get; set; }
        public string gethErrorLog { get; set; }

        public string restartCommand { get; set; }
        public string restartArguments { get; set; }

        public int healthCheckBlockDelay { get; set; }
        public float healthCheckRAM { get; set; }
        public int healthCheckTimeFrame { get; set; } = 1;
        public float healthCheckCPU { get; set; } = 100;
        public float healthCheckDiskSpace { get; set; }
        public string healthCheckDiskName { get; set; }


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

        public string GetPublicGethConnectionString()
        {
            publicGEthConnectionStrings = publicGEthConnectionStrings?.Where(x => !x.IsNullOrWhitespace())?.ToArray();

            if (publicGEthConnectionStrings.IsNullOrEmpty())
                throw new Exception("publicGEthConnectionStrings were not defined in 'GEthManagerConfig.json'");

            return publicGEthConnectionStrings[RandomEx.Next(0, publicGEthConnectionStrings.Length)];
        }

        public string GetPrivateGethConnectionString()
        {
            if (privateGEthConnectionString.IsNullOrWhitespace())
                throw new Exception("privateGEthConnectionString were not defined in 'GEthManagerConfig.json'");

            return privateGEthConnectionString;
        }
    }
}
