using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using System.Collections.Generic;

namespace Neo.TestingEngine
{
    public class SmartContractTest
    {
        public readonly UInt160 scriptHash = null;
        public readonly string nefPath = null;
        public readonly string methodName;
        public JArray methodParameters;
        public Dictionary<StorageKey, StorageItem> storage;
        public List<TestContract> contracts;
        public uint currentHeight = 0;
        public UInt160[] signers;
        public Block[] blocks;
        public Transaction currentTx;

        public SmartContractTest(string path, string method, JArray parameters) : this(method, parameters)
        {
            nefPath = path;
        }

        public SmartContractTest(UInt160 scriptHash, string method, JArray parameters) : this(method, parameters)
        {
            this.scriptHash = scriptHash;
        }

        private SmartContractTest(string method, JArray parameters)
        {
            methodName = method;
            methodParameters = parameters;
            storage = new Dictionary<StorageKey, StorageItem>();
            contracts = new List<TestContract>();
            signers = new UInt160[] { };
            blocks = new Block[] { };
        }
    }
}
