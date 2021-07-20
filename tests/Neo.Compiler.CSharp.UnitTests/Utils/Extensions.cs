using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests.Utils
{
    public static class Extensions
    {
        public static void ContractAdd(this DataCache snapshot, ContractState contract)
        {
            var key = new KeyBuilder(-1, 8).Add(contract.Hash);
            snapshot.Add(key, new StorageItem(contract));
        }

        public static void DeployNativeContracts(this DataCache snapshot, Block persistingBlock = null)
        {
            persistingBlock ??= new NeoSystem(ProtocolSettings.Default).GenesisBlock;

            var method = typeof(SmartContract.Native.ContractManagement).GetMethod("OnPersist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var engine = new TestEngine(TriggerType.OnPersist, null, snapshot, persistingBlock);
            method.Invoke(SmartContract.Native.NativeContract.ContractManagement, new object[] { engine });

            var method2 = typeof(SmartContract.Native.LedgerContract).GetMethod("PostPersist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method2.Invoke(SmartContract.Native.NativeContract.Ledger, new object[] { engine });
        }
    }
}
