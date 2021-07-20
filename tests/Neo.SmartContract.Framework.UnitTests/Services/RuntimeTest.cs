using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract.Framework.UnitTests.Utils;
using Neo.SmartContract.Manifest;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.IO;
using Neo.Wallets;
using System.Numerics;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class RuntimeTest
    {
        class DummyVerificable : IVerifiable
        {
            public Witness[] Witnesses { get; set; }

            public int Size => 0;

            public void Deserialize(BinaryReader reader) { }

            public void DeserializeUnsigned(BinaryReader reader) { }

            public UInt160[] GetScriptHashesForVerifying(DataCache snapshot)
            {
                return new UInt160[]
                {
                    UInt160.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF")
                };
            }

            public void Serialize(BinaryWriter writer) { }

            public void SerializeUnsigned(BinaryWriter writer) { }
        }

        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(TriggerType.Application, new DummyVerificable(), persistingBlock: new Block()
            {
                Header = new Header()
                {
                    Index = 123,
                    Timestamp = 1234,
                    Witness = new Witness()
                    {
                        InvocationScript = System.Array.Empty<byte>(),
                        VerificationScript = System.Array.Empty<byte>()
                    },
                    NextConsensus = UInt160.Zero,
                    MerkleRoot = UInt256.Zero,
                    PrevHash = UInt256.Zero
                },

                Transactions = System.Array.Empty<Transaction>(),

            });
            _engine.AddEntryScript("./TestClasses/Contract_Runtime.cs");
        }

        [TestMethod]
        public void Test_InvocationCounter()
        {
            _engine.AddEntryScript("./TestClasses/Contract_Runtime.cs");

            // We need a new TestEngine because invocationCounter it's shared between them

            var contract = _engine.EntryScriptHash;
            var engine = new TestEngine(TriggerType.Application, new DummyVerificable(), new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });

            using (ScriptBuilder sb = new())
            {
                // First
                sb.EmitDynamicCall(contract, "getInvocationCounter");
                // Second
                sb.EmitDynamicCall(contract, "getInvocationCounter");
                engine.LoadScript(sb.ToArray());
            }

            // Check

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(2, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x02, item.GetInteger());

            item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x01, item.GetInteger());
        }

        [TestMethod]
        public void Test_Time()
        {
            var result = _engine.ExecuteTestCaseStandard("getTime");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(1234, item.GetInteger());
        }

        [TestMethod]
        public void Test_Platform()
        {
            var result = _engine.ExecuteTestCaseStandard("getPlatform");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            Assert.AreEqual("NEO", item.GetString());
        }

        [TestMethod]
        public void Test_Trigger()
        {
            var result = _engine.ExecuteTestCaseStandard("getTrigger");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual((byte)TriggerType.Application, item.GetInteger());
        }

        [TestMethod]
        public void Test_Random()
        {
            var result = _engine.ExecuteTestCaseStandard("getRandom");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(BigInteger.Parse("33807291057955110206057014245126331960"), item.GetInteger());
        }

        [TestMethod]
        public void Test_GetNetwork()
        {
            var result = _engine.ExecuteTestCaseStandard("getNetwork");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(BigInteger.Parse("5195086"), item.GetInteger());
        }

        [TestMethod]
        public void Test_GasLeft()
        {
            var result = _engine.ExecuteTestCaseStandard("getGasLeft");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
        }

        [TestMethod]
        public void Test_Log()
        {
            var list = new System.Collections.Generic.List<LogEventArgs>();
            var method = new EventHandler<LogEventArgs>((s, e) => list.Add(e));

            ApplicationEngine.Log += method;
            var result = _engine.ExecuteTestCaseStandard("log", new VM.Types.ByteString(Utility.StrictUTF8.GetBytes("LogTest")));
            ApplicationEngine.Log -= method;

            Assert.AreEqual(1, list.Count);

            var item = list[0];
            Assert.AreEqual("LogTest", item.Message);
        }

        [TestMethod]
        public void Test_CheckWitness()
        {
            // True

            var result = _engine.ExecuteTestCaseStandard("checkWitness", new VM.Types.ByteString(
                new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, }
                ));
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Boolean));
            Assert.IsTrue(item.GetBoolean());

            // False

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("checkWitness", new VM.Types.ByteString(
                new byte[] { 0xFA, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, }
                ));
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void Test_GetNotificationsCount()
        {
            _engine.ClearNotifications();
            _engine.SendTestNotification(UInt160.Zero, "", new VM.Types.Array(new StackItem[] { new Integer(0x01) }));
            _engine.SendTestNotification(UInt160.Parse("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"), "", new VM.Types.Array(new StackItem[] { new Integer(0x02) }));

            var result = _engine.ExecuteTestCaseStandard("getNotificationsCount", new VM.Types.ByteString(UInt160.Parse("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF").ToArray()));
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x01, item.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getNotificationsCount", StackItem.Null);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x02, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetNotifications()
        {
            _engine.ClearNotifications();
            _engine.SendTestNotification(UInt160.Zero, "", new VM.Types.Array(new StackItem[] { new Integer(0x01) }));
            _engine.SendTestNotification(UInt160.Parse("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF"), "", new VM.Types.Array(new StackItem[] { new Integer(0x02) }));

            var result = _engine.ExecuteTestCaseStandard("getNotifications", new VM.Types.ByteString(UInt160.Parse("0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF").ToArray()));
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x02, item.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getAllNotifications");
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0x03, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionHash()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionHash");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.ByteString));
            Assert.AreEqual(tx.Hash, new UInt256(item.GetSpan()));
        }

        [TestMethod]
        public void Test_GetTransactionVersion()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionVersion");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            tx.Version = 77;
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.Integer));
            Assert.AreEqual(tx.Version, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionNonce()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionNonce");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.Integer));
            Assert.AreEqual(tx.Nonce, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionSender()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionSender");
            var sender = "NMA2FKN8up2cEwaJgtmAiDrZWB69ApnDfp".ToScriptHash(ProtocolSettings.Default.AddressVersion);
            var tx = BuildTransaction(sender, sb.ToArray());
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.ByteString));
            Assert.AreEqual(tx.Sender, new UInt160(item.GetSpan()));
        }

        [TestMethod]
        public void Test_GetTransactionSystemFee()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionSystemFee");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            tx.SystemFee = 10;
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.Integer));
            Assert.AreEqual(tx.SystemFee, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionNetworkFee()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionNetworkFee");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            tx.NetworkFee = 200;
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.Integer));
            Assert.AreEqual(tx.NetworkFee, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionValidUntilBlock()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionValidUntilBlock");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            tx.ValidUntilBlock = 1111;
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(Neo.VM.Types.Integer));
            Assert.AreEqual(tx.ValidUntilBlock, item.GetInteger());
        }

        [TestMethod]
        public void Test_GetTransactionScript()
        {
            var contract = _engine.EntryScriptHash;

            using ScriptBuilder sb = new();
            sb.EmitDynamicCall(contract, "getTransactionScript");

            var tx = BuildTransaction(UInt160.Zero, sb.ToArray());
            tx.NetworkFee = 200;
            var engine = new TestEngine(TriggerType.Application, tx, new TestDataCache());
            engine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = contract,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest),
            });
            engine.LoadScript(sb.ToArray());

            Assert.AreEqual(VMState.HALT, engine.Execute());
            Assert.AreEqual(1, engine.ResultStack.Count);

            var item = engine.ResultStack.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteString));
            Assert.AreEqual(tx.Script.ToHexString(), item.GetSpan().ToHexString());
        }

        private static Transaction BuildTransaction(UInt160 sender, byte[] script)
        {
            Transaction tx = new()
            {
                Script = script,
                Nonce = (uint)new Random().Next(1000, 9999),
                Signers = new Signer[]
                {
                    new() { Account = sender, Scopes = WitnessScope.Global }
                },
                Attributes = System.Array.Empty<TransactionAttribute>()
            };
            return tx;
        }
    }
}
