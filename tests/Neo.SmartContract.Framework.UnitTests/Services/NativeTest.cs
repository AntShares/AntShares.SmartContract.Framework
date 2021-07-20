using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.Cryptography.ECC;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class NativeTest
    {
        private TestEngine _engine;
        private readonly byte[] pubKey = "03ea01cb94bdaf0cd1c01b159d474f9604f4af35a3e2196f6bdfdb33b2aa4961fa".HexToBytes();
        private readonly byte[] account = new byte[] { 0xf6, 0x64, 0x43, 0x49, 0x8d, 0x38, 0x78, 0xd3, 0x2b, 0x99, 0x4e, 0x4e, 0x12, 0x83, 0xc6, 0x93, 0x44, 0x21, 0xda, 0xfe };

        [TestInitialize]
        public void Init()
        {
            // Deploy native contracts
            var block = new Block()
            {
                Header = new Header()
                {
                    Index = 0,
                    Witness = new Witness()
                    {
                        InvocationScript = System.Array.Empty<byte>(),
                        VerificationScript = Contract.CreateSignatureRedeemScript(ECPoint.FromBytes(pubKey, ECCurve.Secp256k1))
                    },
                    NextConsensus = UInt160.Zero,
                    MerkleRoot = UInt256.Zero,
                    PrevHash = UInt256.Zero
                },
                Transactions = System.Array.Empty<Transaction>(),
            };

            _engine = new TestEngine(TriggerType.Application, block, new TestDataCache(block));
            _engine.Reset();
            _engine.AddEntryScript("./TestClasses/Contract_Native.cs");
        }

        [TestMethod]
        public void Test_NEO()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("NEO_Decimals");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0, item.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_BalanceOf", account);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0, item.GetInteger());

            // Before RegisterCandidate
            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_GetCandidates");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Array));
            Assert.AreEqual(0, ((Array)item).Count);

            // RegisterCandidate
            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_RegisterCandidate", pubKey);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.AreEqual(true, item.GetBoolean());

            // After RegisterCandidate
            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_GetCandidates");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Array));
            Assert.AreEqual(1, ((Array)item).Count);
            var candidate = ((Array)item)[0];
            Assert.IsInstanceOfType(candidate, typeof(Struct));
            var candidatePubKey = ((Struct)candidate)[0];
            var candidateVotes = ((Struct)candidate)[1];
            Assert.IsInstanceOfType(candidatePubKey, typeof(ByteString));
            Assert.AreEqual(true, candidatePubKey.Equals((ByteString)pubKey));
            Assert.IsInstanceOfType(candidateVotes, typeof(Integer));
            Assert.AreEqual(0, candidateVotes.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_Transfer", account, account, 0);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.AreEqual(false, item.GetBoolean());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_UnclaimedGas", account, 0);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(0, item.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_GetGasPerBlock");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(500000000, item.GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("NEO_GetAccountState", account);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Null));
        }

        [TestMethod]
        public void Test_GAS()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("GAS_Decimals");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(8, item.GetInteger());
        }

        [TestMethod]
        public void Test_Policy()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("Policy_GetFeePerByte");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(1000L, item.GetInteger());

            _engine.Reset();

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("Policy_IsBlocked", account);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.AreEqual(false, item.GetBoolean());
        }
    }
}
