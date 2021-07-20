using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using System.Linq;

namespace Neo.TestingEngine
{
    public static class TestExtensions
    {
        private const byte Prefix_NextAvailableId = 15;

        public static void ContractAdd(this DataCache snapshot, ContractState contract)
        {
            var key = new KeyBuilder(-1, 8).Add(contract.Hash);
            snapshot.Add(key, new StorageItem(contract));
        }

        public static bool ContainsContract(this DataCache snapshot, UInt160 hash)
        {
            return NativeContract.ContractManagement.GetContract(snapshot, hash) != null;
        }

        public static void DeleteContract(this DataCache snapshot, UInt160 hash)
        {
            var contract = NativeContract.ContractManagement.GetContract(snapshot, hash);
            if (contract != null)
            {
                var key = new KeyBuilder(NativeContract.ContractManagement.Id, 8).Add(hash);
                snapshot.Delete(key);
            }
        }

        public static void DeployNativeContracts(this DataCache snapshot, Block persistingBlock = null)
        {
            persistingBlock ??= new NeoSystem(ProtocolSettings.Default).GenesisBlock;
            var method = typeof(ContractManagement).GetMethod("OnPersist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var engine = new TestEngine(TriggerType.OnPersist, null, snapshot, persistingBlock);
            method.Invoke(NativeContract.ContractManagement, new object[] { engine });

            var method2 = typeof(LedgerContract).GetMethod("PostPersist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method2.Invoke(NativeContract.Ledger, new object[] { engine });
        }

        public static bool TryContractAdd(this DataCache snapshot, ContractState contract)
        {
            var key = NativeContract.ContractManagement.CreateStorageKey(8, contract.Hash);
            if (snapshot.Contains(key))
            {
                return false;
            }

            snapshot.Add(key, new StorageItem(contract));
            return true;
        }
        public static int GetNextAvailableId(this DataCache snapshot)
        {
            StorageItem item = snapshot.GetAndChange(NativeContract.ContractManagement.CreateStorageKey(Prefix_NextAvailableId));
            item.Add(1);
            return NativeContract.ContractManagement.ListContracts(snapshot).ToList().Where(state => state.Id >= 0).Count();
        }
    }
}
