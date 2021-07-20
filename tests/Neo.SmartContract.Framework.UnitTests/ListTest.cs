using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.IO.Json;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.SmartContract.Framework.UnitTests
{
    [TestClass]
    public class ListTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(snapshot: new TestDataCache());
            _engine.AddEntryScript("./TestClasses/Contract_List.cs");
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
        public void TestAdd()
        {
            _engine.Reset();
            StackItem count = 4;
            var result = _engine.ExecuteTestCaseStandard("testAdd", count);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var item = result.Pop();
            var json = ParseJson(item);

            Assert.IsTrue(json is JArray);
            var jarray = (JArray)json;
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(jarray[i] is JNumber);
                Assert.AreEqual(i, jarray[i].AsNumber());
            }
        }

        [TestMethod]
        public void TestRemoveAt()
        {
            _engine.Reset();
            StackItem count = 5;
            StackItem removeAt = 2;
            var result = _engine.ExecuteTestCaseStandard("testRemoveAt", count, removeAt);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var item = result.Pop();
            var json = ParseJson(item);

            Assert.IsTrue(json is JArray);
            var jarray = (JArray)json;
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(jarray[i] is JNumber);
                Assert.AreEqual(i < 2 ? i : i + 1, jarray[i].AsNumber());
            }
        }

        [TestMethod]
        public void TestClear()
        {
            _engine.Reset();
            StackItem count = 4;
            var result = _engine.ExecuteTestCaseStandard("testClear", count);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var item = result.Pop();
            var json = ParseJson(item);

            Assert.IsTrue(json is JArray);
            var jarray = (JArray)json;
            Assert.AreEqual(0, jarray.Count);
        }

        [TestMethod]
        public void TestArrayConvert()
        {
            _engine.Reset();
            StackItem count = 4;
            var result = _engine.ExecuteTestCaseStandard("testArrayConvert", count);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);
            var item = result.Pop();
            Assert.IsTrue(item is Array);
            var array = (Array)item;
            Assert.AreEqual(4, array.Count);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsTrue(array[i] is Integer);
                Assert.AreEqual(i, array[i].GetInteger());
            }
        }

        static JObject ParseJson(StackItem item)
        {
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            var json = System.Text.Encoding.UTF8.GetString(item.GetSpan());
            return JObject.Parse(json);
        }
    }
}
