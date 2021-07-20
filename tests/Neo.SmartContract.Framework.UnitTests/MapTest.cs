using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.SmartContract.Framework.UnitTests
{
    [TestClass]
    public class MapTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(snapshot: new TestDataCache());
            _engine.AddEntryScript("./TestClasses/Contract_Map.cs");
        }

        [TestMethod]
        public void TestCount()
        {
            _engine.Reset();
            StackItem count = 4;
            var result = _engine.ExecuteTestCaseStandard("testCount", count);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(4, (item as Integer).GetInteger());
        }

        [TestMethod]
        public void TestByteArray()
        {
            _engine.Reset();
            StackItem key = System.Text.Encoding.ASCII.GetBytes("a");
            var result = _engine.ExecuteTestCaseStandard("testByteArray", key);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteString));
            // Except: {"a":"teststring2"}
            Assert.AreEqual("7b2261223a2274657374737472696e6732227d", (item as ByteString).GetSpan().ToHexString());
        }

        [TestMethod]
        public void TestClear()
        {
            _engine.Reset();
            StackItem key = System.Text.Encoding.ASCII.GetBytes("a");
            var result = _engine.ExecuteTestCaseStandard("testClear", key);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteString));
            // Except: {}
            Assert.AreEqual("7b7d", (item as ByteString).GetSpan().ToHexString());
        }


        [TestMethod]
        public void TestByteArray2()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testByteArray2");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            // Except: {"\u0001\u0001":"\u0022\u0022"}
            Assert.AreEqual("7b225c75303030315c7530303031223a225c75303032325c7530303232227d", (item as VM.Types.ByteString).GetSpan().ToHexString());
        }

        [TestMethod]
        public void TestUnicode()
        {
            _engine.Reset();
            StackItem key = Utility.StrictUTF8.GetBytes("中");
            var result = _engine.ExecuteTestCaseStandard("testUnicode", key);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            // Except: {"\u4E2D":"129840test10022939"}
            Assert.AreEqual("7b225c7534453244223a22313239383430746573743130303232393339227d", (item as VM.Types.ByteString).GetSpan().ToHexString());
        }

        [TestMethod]
        public void TestUnicodeValue()
        {
            _engine.Reset();
            StackItem value = Utility.StrictUTF8.GetBytes("文");
            var result = _engine.ExecuteTestCaseStandard("testUnicodeValue", value);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            // Except: {"ab":"\u6587"}
            Assert.AreEqual("7b226162223a225c7536353837227d", (item as VM.Types.ByteString).GetSpan().ToHexString());
        }

        [TestMethod]
        public void TestUnicodeKeyValue()
        {
            _engine.Reset();
            StackItem key = Utility.StrictUTF8.GetBytes("中");
            StackItem value = Utility.StrictUTF8.GetBytes("文");
            var result = _engine.ExecuteTestCaseStandard("testUnicodeKeyValue", key, value);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            // Except: {"\u4E2D":"\u6587"}
            Assert.AreEqual("7b225c7534453244223a225c7536353837227d", (item as VM.Types.ByteString).GetSpan().ToHexString());
        }

        [TestMethod]
        public void TestInt()
        {
            _engine.Reset();
            StackItem key = 1;
            _engine.ExecuteTestCaseStandard("testInt", key);
            // Int cannot be used as the key for serializing Map
            Assert.AreEqual(VMState.FAULT, _engine.State);
        }

        [TestMethod]
        public void TestBool()
        {
            _engine.Reset();
            StackItem key = true;
            _engine.ExecuteTestCaseStandard("testBool", key);
            // Bool cannot be used as the key for serializing Map
            Assert.AreEqual(VMState.FAULT, _engine.State);
        }

        [TestMethod]
        public void TestDeserialize()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testDeserialize", "a");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Map));
            var map = item as Map;
            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey("a"));
            Assert.AreEqual((VM.Types.ByteString)"testdeserialize", map["a"]);
        }

        [TestMethod]
        public void TestUInt160KeyDeserialize()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testuint160Key");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Map));
            var map = item as Map;
            Assert.AreEqual(1, map.Count);
            Assert.IsTrue(map.ContainsKey(new byte[20]));
            Assert.AreEqual(1, map[new byte[20]]);
        }
    }
}
