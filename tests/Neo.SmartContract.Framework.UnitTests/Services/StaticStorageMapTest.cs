using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class StaticStorageMapTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            var system = TestBlockchain.TheNeoSystem;
            var snapshot = system.GetSnapshot().CreateSnapshot();

            _engine = new TestEngine(snapshot: snapshot);
            _engine.AddEntryScript("./TestClasses/Contract_StaticStorageMap.cs");
            snapshot.ContractAdd(new ContractState()
            {
                Hash = _engine.EntryScriptHash,
                Nef = _engine.Nef,
                Manifest = new Manifest.ContractManifest()
            });
        }

        [TestMethod]
        public void Test_Storage()
        {
            _engine.Reset();
            _engine.ExecuteTestCaseStandard("put2", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("get2", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(3, result.Pop());
        }

        [TestMethod]
        public void Test_StaticStorageMap()
        {
            _engine.Reset();
            _engine.ExecuteTestCaseStandard("put", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("get", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Pop());

            _engine.Reset();
            _engine.ExecuteTestCaseStandard("putReadonly", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("getReadonly", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(2, result.Pop());
        }

        [TestMethod]
        public void Test_StaticStorageMapBytePrefix()
        {
            _engine.Reset();
            _engine.ExecuteTestCaseStandard("teststoragemap_Putbyteprefix", 0);
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("teststoragemap_Getbyteprefix", 0);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(123, result.Pop());

            _engine.Reset();
            _engine.ExecuteTestCaseStandard("teststoragemap_Putbyteprefix", 255);
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("teststoragemap_Getbyteprefix", 255);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(123, result.Pop());

            _engine.Reset();
            _engine.ExecuteTestCaseStandard("teststoragemap_Putbyteprefix", -128);
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("teststoragemap_Getbyteprefix", -128);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(123, result.Pop());


            _engine.Reset();
            _engine.ExecuteTestCaseStandard("teststoragemap_Putbyteprefix", 127);
            Assert.AreEqual(VMState.HALT, _engine.State);

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("teststoragemap_Getbyteprefix", 127);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(123, result.Pop());

            _engine.Reset();
            _engine.ExecuteTestCaseStandard("teststoragemap_Putbyteprefix", 256);
            Assert.AreEqual(VMState.FAULT, _engine.State);
        }
    }
}
