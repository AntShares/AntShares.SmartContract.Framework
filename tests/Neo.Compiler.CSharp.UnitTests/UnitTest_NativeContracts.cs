using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class Contract_NativeContracts
    {
        private TestDataCache snapshot;
        private Block genesisBlock;

        [TestInitialize]
        public void Test_Init()
        {
            snapshot = new TestDataCache();
            genesisBlock = new NeoSystem(ProtocolSettings.Default).GenesisBlock;
        }

        [TestMethod]
        public void TestHashes()
        {
            Assert.AreEqual(NativeContract.StdLib.Hash.ToString(), "0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0");
            Assert.AreEqual(NativeContract.CryptoLib.Hash.ToString(), "0x726cb6e0cd8628a1350a611384688911ab75f51b");
            Assert.AreEqual(NativeContract.ContractManagement.Hash.ToString(), "0xfffdc93764dbaddd97c48f252a53ea4643faa3fd");
            Assert.AreEqual(NativeContract.RoleManagement.Hash.ToString(), "0x49cf4e5378ffcd4dec034fd98a174c5491e395e2");
            Assert.AreEqual(NativeContract.Oracle.Hash.ToString(), "0xfe924b7cfe89ddd271abaf7210a80a7e11178758");
            Assert.AreEqual(NativeContract.NEO.Hash.ToString(), "0xef4073a0f2b305a38ec4050e4d3d28bc40ea63f5");
            Assert.AreEqual(NativeContract.GAS.Hash.ToString(), "0xd2a4cff31913016155e38e474a2c06d08be276cf");
            Assert.AreEqual(NativeContract.Policy.Hash.ToString(), "0xcc5e4edd9f5f8dba8bb65734541df7a1c081c67b");
            Assert.AreEqual(NativeContract.Ledger.Hash.ToString(), "0xda65b600f7124ce6c79950c1772a36403104f2be");
        }

        [TestMethod]
        public void Test_Oracle()
        {
            var testengine = new TestEngine(TriggerType.Application, null, snapshot);
            testengine.AddEntryScript("./TestClasses/Contract_NativeContracts.cs");

            // Minimum Response Fee

            var result = testengine.ExecuteTestCaseStandard("oracleMinimumResponseFee");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var entry = result.Pop();

            Assert.AreEqual(0_10000000u, entry.GetInteger());
        }

        [TestMethod]
        public void Test_Designation()
        {
            var testengine = new TestEngine(TriggerType.Application, null, snapshot);
            testengine.AddEntryScript("./TestClasses/Contract_NativeContracts.cs");

            // getOracleNodes

            var result = testengine.ExecuteTestCaseStandard("getOracleNodes");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var entry = result.Pop();

            Assert.AreEqual(0, (entry as VM.Types.Array).Count);
        }

        [TestMethod]
        public void Test_NEO()
        {
            var testengine = new TestEngine(TriggerType.Application, null, snapshot);
            testengine.AddEntryScript("./TestClasses/Contract_NativeContracts.cs");

            // NeoSymbol

            var result = testengine.ExecuteTestCaseStandard("nEOSymbol");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var entry = result.Pop();
            Assert.AreEqual("NEO", entry.GetString());

            // NeoHash

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nEOHash");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            entry = result.Pop();
            Assert.IsTrue(entry is VM.Types.ByteString);
            var hash = new UInt160((VM.Types.ByteString)entry);
            Assert.AreEqual(NativeContract.NEO.Hash, hash);
        }

        [TestMethod]
        public void Test_GAS()
        {
            var testengine = new TestEngine(TriggerType.Application, null, snapshot);
            testengine.AddEntryScript("./TestClasses/Contract_NativeContracts.cs");

            var result = testengine.ExecuteTestCaseStandard("gASSymbol");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var entry = result.Pop();

            Assert.AreEqual("GAS", entry.GetString());
        }

        [TestMethod]
        public void Test_Ledger()
        {
            var testengine = new TestEngine(TriggerType.Application, null, snapshot, persistingBlock: genesisBlock);
            testengine.AddEntryScript("./TestClasses/Contract_NativeContracts.cs");

            var result = testengine.ExecuteTestCaseStandard("ledgerHash");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var entry = result.Pop();
            Assert.IsTrue(entry is VM.Types.ByteString);
            var hash = new UInt160((VM.Types.ByteString)entry);
            Assert.AreEqual(NativeContract.Ledger.Hash.ToString(), hash.ToString());

            testengine.Reset();

            result = testengine.ExecuteTestCaseStandard("ledgerCurrentIndex");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            entry = result.Pop();

            Assert.AreEqual(0, entry.GetInteger());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("ledgerCurrentHash");

            Assert.AreEqual(VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            entry = result.Pop();
            Assert.IsTrue(entry is VM.Types.ByteString);
            var blockHash = new UInt256((VM.Types.ByteString)entry);
            Assert.AreEqual(genesisBlock.Hash.ToString(), blockHash.ToString());
        }
    }
}
