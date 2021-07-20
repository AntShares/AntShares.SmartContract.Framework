using System.Collections.Generic;
using System.Linq;
using Neo.IO;
using Neo.Ledger;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;

namespace Neo.TestingEngine
{
    public static class TestBlockchain
    {
        public static readonly NeoSystem TheNeoSystem;

        const byte Prefix_Block = 5;
        const byte Prefix_BlockHash = 9;
        const byte Prefix_Transaction = 11;
        const byte Prefix_CurrentBlock = 12;

        static TestBlockchain()
        {
            TheNeoSystem = new NeoSystem(ProtocolSettings.Default);
        }

        public static StorageKey CreateStorageKey(this NativeContract contract, byte prefix, ISerializable key = null)
        {
            var k = new KeyBuilder(contract.Id, prefix);
            if (key != null) k = k.Add(key);
            return k;
        }

        public static StorageKey CreateStorageKey(this NativeContract contract, byte prefix, uint value)
        {
            return new KeyBuilder(contract.Id, prefix).AddBigEndian(value);
        }

        public static List<(StorageKey, StorageItem)> GetStorageOnly(this DataCache snapshot)
        {
            var nativeTokens = new int[]
            {
                NativeContract.GAS.Id, NativeContract.NEO.Id, NativeContract.ContractManagement.Id, NativeContract.Oracle.Id
            };

            return snapshot.Find().Where(pair => pair.Key.Id >= 0 || nativeTokens.Contains(pair.Key.Id)).ToList();
        }

        public static void BlocksDelete(this DataCache snapshot, UInt256 hash)
        {
            snapshot.Delete(NativeContract.Ledger.CreateStorageKey(Prefix_BlockHash, hash));
            snapshot.Delete(NativeContract.Ledger.CreateStorageKey(Prefix_Block, hash));
        }

        public static void TransactionAdd(this DataCache snapshot, params TransactionState[] txs)
        {
            foreach (TransactionState tx in txs)
            {
                snapshot.Add(NativeContract.Ledger.CreateStorageKey(Prefix_Transaction, tx.Transaction.Hash), new StorageItem(tx));
            }
        }

        public static void BlocksDelete(this DataCache snapshot, UInt256 hash, TrimmedBlock block)
        {
            snapshot.Delete(NativeContract.Ledger.CreateStorageKey(Prefix_BlockHash, block.Index));
            snapshot.Delete(NativeContract.Ledger.CreateStorageKey(Prefix_Block, hash));
        }

        public static void BlocksAdd(this DataCache snapshot, UInt256 hash, TrimmedBlock block)
        {
            snapshot.Add(NativeContract.Ledger.CreateStorageKey(Prefix_BlockHash, block.Index), new StorageItem(hash.ToArray()));
            snapshot.Add(NativeContract.Ledger.CreateStorageKey(Prefix_Block, hash), new StorageItem(block.ToArray()));
        }

        public static void UpdateChangedBlocks(this DataCache snapshot, UInt256 oldHash, UInt256 newHash, TrimmedBlock block)
        {
            if (snapshot.Contains(NativeContract.Ledger.CreateStorageKey(Prefix_Block, oldHash)))
            {
                var oldBlock = NativeContract.Ledger.GetBlock(snapshot, oldHash);
                snapshot.BlocksDelete(oldHash, oldBlock.Trim());
            }

            if (snapshot.Contains(NativeContract.Ledger.CreateStorageKey(Prefix_Block, newHash)))
            {
                snapshot.BlocksDelete(newHash, block);
            }
            snapshot.BlocksAdd(newHash, block);
        }

        public static TrimmedBlock Trim(this Block block)
        {
            return new TrimmedBlock
            {
                Header = new Header()
                {
                    Version = block.Version,
                    PrevHash = block.PrevHash,
                    MerkleRoot = block.MerkleRoot,
                    Nonce = block.Nonce,
                    Timestamp = block.Timestamp,
                    Index = block.Index,
                    NextConsensus = block.NextConsensus,
                    Witness = block.Witness,
                },
                Hashes = block.Transactions.Select(p => p.Hash).ToArray(),
            };
        }

        public static Block TryGetBlock(this DataCache snapshot, uint index)
        {
            return NativeContract.Ledger.GetBlock(snapshot, index);
        }

        public static void BlocksAddOrUpdate(this DataCache snapshot, UInt256 hash, TrimmedBlock block)
        {
            var existingBlock = NativeContract.Ledger.GetBlock(snapshot, block.Index);
            if (existingBlock != null)
            {
                snapshot.UpdateChangedBlocks(existingBlock.Hash, hash, block);
            }
            else
            {
                snapshot.BlocksAdd(hash, block);
            }
        }

        public static List<TrimmedBlock> Blocks(this DataCache snapshot)
        {
            var blockKey = NativeContract.Ledger.CreateStorageKey(Prefix_Block);
            return snapshot.Find(blockKey.ToArray())
                .Select(item => item.Value.Value.AsSerializable<TrimmedBlock>())
                .OrderBy(b => b.Index).ToList();
        }

        public static List<UInt256> BlocksIndex(this DataCache snapshot)
        {
            var blockKey = NativeContract.Ledger.CreateStorageKey(Prefix_BlockHash);
            return snapshot.Find(blockKey.ToArray())
                .Select(item => new UInt256(item.Value.ToArray()))
                .ToList();
        }

        public static Block GetLastBlock(this DataCache snapshot)
        {
            var trim = snapshot.Blocks().Last();
            return NativeContract.Ledger.GetBlock(snapshot, trim.Hash);
        }

        public static void SetCurrentBlockHash(this DataCache snapshot, uint index, UInt256 hash)
        {
            var key = NativeContract.Ledger.CreateStorageKey(Prefix_CurrentBlock);
            var item = snapshot.GetAndChange(key, () => new StorageItem());

            var hashIndex = new TestHashIndexState();
            var stack = BinarySerializer.Deserialize(item.Value, ExecutionEngineLimits.Default);
            hashIndex.FromStackItem(stack);
            hashIndex.Hash = hash;
            hashIndex.Index = index;

            item.Value = BinarySerializer.Serialize(hashIndex.ToStackItem(null), ExecutionEngineLimits.Default.MaxStackSize);
        }

        public static void TransactionAddOrUpdate(this DataCache snapshot, params TransactionState[] txs)
        {
            foreach (TransactionState tx in txs)
            {
                var key = NativeContract.Ledger.CreateStorageKey(Prefix_Transaction, tx.Transaction.Hash);

                if (snapshot.Contains(key))
                {
                    snapshot.Delete(key);
                }
                snapshot.Add(key, new StorageItem(tx));
            }
        }

        public static bool ContainsTransaction(this DataCache snapshot, Transaction tx)
        {
            var key = NativeContract.Ledger.CreateStorageKey(Prefix_Transaction, tx.Hash);
            return snapshot.Contains(key);
        }

        public static void AddOrUpdateTransactions(this DataCache snapshot, Transaction[] txs, int blockIndex = -1)
        {
            uint index = blockIndex >= 0 ? (uint)blockIndex : NativeContract.Ledger.CurrentIndex(snapshot);
            snapshot.AddOrUpdateTransactions(txs, index);
        }

        public static void AddOrUpdateTransactions(this DataCache snapshot, Transaction[] txs, uint index)
        {
            var states = new List<TransactionState>();
            foreach (var tx in txs)
            {
                var transaction = tx;
                if (snapshot.ContainsTransaction(tx))
                {
                    transaction = NativeContract.Ledger.GetTransaction(snapshot, tx.Hash);
                }

                var state = new TransactionState()
                {
                    BlockIndex = index,
                    Transaction = transaction
                };
                states.Add(state);
            }

            snapshot.TransactionAddOrUpdate(states.ToArray());
        }
    }
}
