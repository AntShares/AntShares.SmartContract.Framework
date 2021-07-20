using Neo.Cryptography.ECC;
using Neo.IO;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.Wallets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.TestingEngine
{
    public class Engine
    {
        private static Engine instance = null;
        public static Engine Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Engine();
                }
                return instance;
            }
        }

        private TestEngine engine = null;
        private Transaction currentTx = null;
        private ECPoint PubKey => wallet.DefaultAccount.GetKey().PublicKey;
        private TestWallet wallet = null;

        private Engine()
        {
            var _ = TestBlockchain.TheNeoSystem;
            wallet = new TestWallet();
            Reset();
        }

        public uint Height => NativeContract.Ledger.CurrentIndex(engine.Snapshot);

        public DataCache Snaptshot => engine.Snapshot;

        public void Reset()
        {
            engine = SetupNativeContracts();
        }

        public Engine SetEntryScript(string path)
        {
            AddSmartContract(path);
            return this;
        }

        public Engine SetEntryScript(UInt160 contractHash)
        {
            engine.AddEntryScript(contractHash);
            return this;
        }

        public Engine AddSmartContract(TestContract contract)
        {
            var state = AddSmartContract(contract.nefPath);
            contract.buildScript = state;
            return this;
        }

        private object AddSmartContract(string path)
        {
            engine.AddEntryScript(path);

            if (engine.ScriptContext?.Success == true)
            {
                var hash = engine.Nef.Script.ToScriptHash();
                var snapshot = engine.Snapshot;

                ContractState state;
                if (!snapshot.ContainsContract(hash))
                {
                    DeployContract(engine.ScriptContext);
                    state = NativeContract.ContractManagement.GetContract(snapshot, hash);
                    if (state is null)
                    {
                        state = new ContractState()
                        {
                            Id = snapshot.GetNextAvailableId(),
                            Hash = hash,
                            Nef = engine.Nef,
                            Manifest = ContractManifest.FromJson(engine.Manifest),
                        };
                        snapshot.TryContractAdd(state);
                    }
                }
                else
                {
                    state = NativeContract.ContractManagement.GetContract(snapshot, hash);
                    engine.AddEntryScript(new BuildScript(state.Nef, state.Manifest.ToJson(), hash));
                }
            }
            return engine.ScriptContext;
        }

        private void DeployContract(BuildScript scriptContext)
        {
            var deploy_method_name = "deploy";
            var deploy_args = new ContractParameter[]
            {
                new ContractParameter(ContractParameterType.ByteArray) {
                    Value = scriptContext.Nef.ToArray()
                },
                new ContractParameter(ContractParameterType.ByteArray) {
                    Value = scriptContext.Manifest.ToByteArray(false)
                },
                new ContractParameter(ContractParameterType.Any)
            };

            engine.AddEntryScript(NativeContract.ContractManagement.Hash);
            byte[] script;
            using (ScriptBuilder scriptBuilder = new ScriptBuilder())
            {
                scriptBuilder.EmitDynamicCall(NativeContract.ContractManagement.Hash, deploy_method_name, deploy_args);
                script = scriptBuilder.ToArray();
            }
            var stackItemsArgs = deploy_args.Select(a => a.ToStackItem()).ToArray();
            engine.RunNativeContract(script, deploy_method_name, stackItemsArgs);

            if (engine.State == VMState.FAULT && engine.FaultException.Message?.StartsWith("Contract Already Exists") != true)
            {
                // deploying a contract already deployed is the only error expected to happen here
                throw engine.FaultException;
            }

            engine.SetContext(scriptContext);
        }

        public Engine IncreaseBlockCount(uint newHeight)
        {
            var snapshot = (TestDataCache)engine.Snapshot;
            if (snapshot.Blocks().Count <= newHeight)
            {
                Block newBlock;
                Block lastBlock = null;
                if (snapshot.Blocks().Count == 0)
                {
                    newBlock = TestBlockchain.TheNeoSystem.GenesisBlock;
                    snapshot.AddOrUpdateTransactions(newBlock.Transactions, newBlock.Index);
                }
                else
                {
                    newBlock = CreateBlock();
                }

                while (snapshot.Blocks().Count <= newHeight)
                {
                    var hash = newBlock.Hash;
                    var trim = newBlock.Trim();
                    snapshot.BlocksAddOrUpdate(hash, trim);
                    lastBlock = newBlock;
                    newBlock = CreateBlock();
                }

                snapshot.SetCurrentBlockHash(lastBlock.Index, lastBlock.Hash);
            }
            return this;
        }

        public Engine SetStorage(Dictionary<StorageKey, StorageItem> storage)
        {
            if (engine.Snapshot is TestDataCache snapshot)
            {
                foreach (var (key, value) in storage)
                {
                    snapshot.AddForTest(key, value);
                }
            }
            return this;
        }

        public Engine SetSigners(UInt160[] signerAccounts)
        {
            if (signerAccounts.Length > 0)
            {
                var newSigners = new List<Signer>();
                foreach (var account in signerAccounts)
                {
                    var signer = new Signer() { Account = account, Scopes = WitnessScope.CalledByEntry };
                    newSigners.Add(signer);
                    wallet.AddSignerAccount(account);
                }
                currentTx.Signers = newSigners.Concat(currentTx.Signers).ToArray();
            }
            return this;
        }

        internal void SetTxAttributes(TransactionAttribute[] attributes)
        {
            currentTx.Attributes = attributes.Where(attr => attr != null).ToArray();
        }

        public Engine AddBlock(Block block)
        {
            if (engine.Snapshot is TestDataCache snapshot)
            {
                Block currentBlock = null;
                if (Height < block.Index || snapshot.Blocks().Count == 0)
                {
                    IncreaseBlockCount(block.Index);
                    currentBlock = engine.Snapshot.GetLastBlock();
                }
                else
                {
                    currentBlock = NativeContract.Ledger.GetBlock(snapshot, block.Index);
                }

                if (currentBlock != null)
                {
                    var hash = currentBlock.Hash;
                    currentBlock.Header.Timestamp = block.Header.Timestamp;

                    if (currentBlock.Transactions.Length > 0)
                    {
                        var tx = currentBlock.Transactions.ToList();
                        tx.AddRange(block.Transactions);
                        currentBlock.Transactions = tx.ToArray();
                    }
                    else
                    {
                        currentBlock.Transactions = block.Transactions;
                    }

                    foreach (var tx in block.Transactions)
                    {
                        tx.ValidUntilBlock = block.Index + ProtocolSettings.Default.MaxValidUntilBlockIncrement;
                    }

                    var trimmed = currentBlock.Trim();
                    snapshot.UpdateChangedBlocks(hash, trimmed.Hash, trimmed);
                }

                snapshot.AddOrUpdateTransactions(block.Transactions);
            }
            return this;
        }

        public JObject Run(string method, ContractParameter[] args)
        {
            if (engine.Snapshot is TestDataCache snapshot)
            {
                if (snapshot.Blocks().Count == 0)
                {
                    // don't use genesis block as persisting block
                    IncreaseBlockCount(1);
                }

                var lastBlock = snapshot.GetLastBlock();
                if (lastBlock.Index == 0)
                {
                    // don't use genesis block as persisting block
                    IncreaseBlockCount(1);
                    lastBlock = snapshot.GetLastBlock();
                }

                engine.PersistingBlock.Header = lastBlock.Header;
                engine.PersistingBlock.Transactions = lastBlock.Transactions;

                currentTx.ValidUntilBlock = lastBlock.Index;
                snapshot.SetCurrentBlockHash(lastBlock.Index, lastBlock.Hash);
            }

            var stackItemsArgs = args.Select(a => a.ToStackItem()).ToArray();
            if (engine.ScriptContext is BuildNative native)
            {
                byte[] script;
                using (ScriptBuilder scriptBuilder = new ScriptBuilder())
                {
                    scriptBuilder.EmitDynamicCall(native.NativeContract.Hash, method, args);
                    script = scriptBuilder.ToArray();
                }
                engine.RunNativeContract(script, method, stackItemsArgs);
            }
            else
            {
                using (ScriptBuilder scriptBuilder = new ScriptBuilder())
                {
                    scriptBuilder.EmitDynamicCall(engine.EntryScriptHash, method, args);
                    currentTx.Script = scriptBuilder.ToArray();
                }
                engine.ExecuteTestCaseStandard(method, stackItemsArgs);
            }

            currentTx.ValidUntilBlock = engine.Snapshot.GetLastBlock().Index + ProtocolSettings.Default.MaxValidUntilBlockIncrement;
            currentTx.SystemFee = engine.GasConsumed;
            currentTx.NetworkFee = wallet.CalculateNetworkFee(engine.Snapshot, currentTx);

            return engine.ToJson();
        }

        private TestEngine SetupNativeContracts()
        {
            currentTx = new Transaction()
            {
                Attributes = new TransactionAttribute[0],
                Script = new byte[0],
                Signers = new Signer[] { new Signer() { Account = wallet.DefaultAccount.ScriptHash, Scopes = WitnessScope.CalledByEntry } },
                Witnesses = new Witness[0],
                NetworkFee = 1,
                Nonce = 2,
                SystemFee = 3,
                Version = 4
            };
            var persistingBlock = new Block()
            {
                Header = new Header()
                {
                    Index = 0
                }
            };
            TestEngine engine = new TestEngine(TriggerType.Application, currentTx, new TestDataCache(), persistingBlock: persistingBlock);

            engine.ClearNotifications();
            return engine;
        }

        private Block CreateBlock(Block originBlock = null)
        {
            TrimmedBlock trimmedBlock = null;
            var blocks = engine.Snapshot.Blocks();
            if (blocks.Count > 0)
            {
                trimmedBlock = blocks.Last();
            }

            if (trimmedBlock == null)
            {
                trimmedBlock = TestBlockchain.TheNeoSystem.GenesisBlock.Trim();
            }

            var newBlock = new Block()
            {
                Header = new Header()
                {
                    Index = trimmedBlock.Index + 1,
                    Timestamp = trimmedBlock.Header.Timestamp + TestBlockchain.TheNeoSystem.Settings.MillisecondsPerBlock,
                    Nonce = trimmedBlock.Header.Nonce,
                    Witness = new Witness()
                    {
                        InvocationScript = new byte[0],
                        VerificationScript = Contract.CreateSignatureRedeemScript(PubKey)
                    },
                    NextConsensus = trimmedBlock.Header.NextConsensus,
                    MerkleRoot = trimmedBlock.Header.MerkleRoot,
                    PrevHash = trimmedBlock.Hash
                },
                Transactions = new Transaction[0]
            };

            if (originBlock != null)
            {
                newBlock.Header.Timestamp = originBlock.Header.Timestamp;
            }

            return newBlock;
        }
    }
}
