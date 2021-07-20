using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.SmartContract.Manifest;
using Neo.VM;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class StdLibTest
    {
        private TestEngine _engine;
        private UInt160 scriptHash;

        [TestInitialize]
        public void Init()
        {
            var _ = TestBlockchain.TheNeoSystem;
            var snapshot = new TestDataCache(null);

            _engine = new TestEngine(TriggerType.Application, snapshot: snapshot);
            Assert.IsTrue(_engine.AddEntryScript("./TestClasses/Contract_StdLib.cs").Success);
            scriptHash = _engine.Nef.Script.ToScriptHash();

            snapshot.ContractAdd(new ContractState()
            {
                Hash = scriptHash,
                Nef = _engine.Nef,
                Manifest = ContractManifest.FromJson(_engine.Manifest)
            });
        }

        [TestMethod]
        public void AtoiTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("atoi", "-1", 10);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.Integer>();
            Assert.AreEqual(-1, item.GetInteger());
        }

        [TestMethod]
        public void ItoaTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("itoa", -1, 10);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.ByteString>();
            Assert.AreEqual("-1", item.GetString());
        }

        [TestMethod]
        public void Base64DecodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base64Decode", "dGVzdA==");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.Buffer>();
            Assert.AreEqual("test", item.GetString());
        }

        [TestMethod]
        public void Base64EncodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base64Encode", "test");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.ByteString>();
            Assert.AreEqual("dGVzdA==", item.GetString());
        }

        [TestMethod]
        public void Base58DecodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base58Decode", "3yZe7d");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.Buffer>();
            Assert.AreEqual("test", item.GetString());
        }

        [TestMethod]
        public void Base58EncodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base58Encode", "test");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.ByteString>();
            Assert.AreEqual("3yZe7d", item.GetString());
        }

        [TestMethod]
        public void Base58CheckEncodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base58CheckEncode", "test");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.ByteString>();
            Assert.AreEqual("LUC1eAJa5jW", item.GetString());
        }

        [TestMethod]
        public void Base58CheckDecodeTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("base58CheckDecode", "LUC1eAJa5jW");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop<VM.Types.Buffer>();
            Assert.AreEqual("test", item.GetString());
        }

        [TestMethod]
        public void MemoryCompareTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("memoryCompare", "abc", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memoryCompare", "abc", "d");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memoryCompare", "abc", "abc");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(0, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memoryCompare", "abc", "abcd");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());
        }

        [TestMethod]
        public void StringSplitTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("stringSplit1", "a,b,c", ",");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var arr = result.Pop<VM.Types.Array>();
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual("a", arr[0].GetString());
            Assert.AreEqual("b", arr[1].GetString());
            Assert.AreEqual("c", arr[2].GetString());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("stringSplit2", "a,,c", ",", false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            arr = result.Pop<VM.Types.Array>();
            Assert.AreEqual(3, arr.Count);
            Assert.AreEqual("a", arr[0].GetString());
            Assert.AreEqual("", arr[1].GetString());
            Assert.AreEqual("c", arr[2].GetString());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("stringSplit2", "a,,c", ",", true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            arr = result.Pop<VM.Types.Array>();
            Assert.AreEqual(2, arr.Count);
            Assert.AreEqual("a", arr[0].GetString());
            Assert.AreEqual("c", arr[1].GetString());
        }

        [TestMethod]
        public void MemorySearchTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("memorySearch1", "abc", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch1", "abc", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch1", "abc", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch1", "abc", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch1", "abc", "d");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch2", "abc", "c", 0);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch2", "abc", "c", 1);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch2", "abc", "c", 2);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch2", "abc", "c", 3);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch2", "abc", "d", 0);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 0, false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 1, false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 2, false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 3, false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "d", 0, false);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 0, true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 1, true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 2, true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "c", 3, true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2, result.Pop<VM.Types.Integer>().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("memorySearch3", "abc", "d", 0, true);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(-1, result.Pop<VM.Types.Integer>().GetInteger());
        }
    }
}
