using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;
using Neo.VM.Types;
using System.Text;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class JsonTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(snapshot: new TestDataCache());
            _engine.AddEntryScript("./TestClasses/Contract_Json.cs");
        }

        [TestMethod]
        public void Test_SerializeDeserialize()
        {
            // Empty Serialize

            var result = _engine.ExecuteTestCaseStandard("serialize");
            Assert.AreEqual(VMState.FAULT, _engine.State);
            Assert.AreEqual(0, result.Count);

            // Empty Serialize

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("deserialize");
            Assert.AreEqual(VMState.FAULT, _engine.State);
            Assert.AreEqual(0, result.Count);

            // Serialize

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("serialize", new Array(new StackItem[]{
                 StackItem.Null, new Boolean(true), new ByteString(Encoding.ASCII.GetBytes("asd"))
            }));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.ByteString));
            Assert.AreEqual("[null,true,\"asd\"]", item.GetString());

            // Deserialize

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("deserialize", new ByteString(Encoding.ASCII.GetBytes("[null,true,\"asd\"]")));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Array));

            var entry = ((Array)item)[0];
            Assert.IsInstanceOfType(entry, typeof(Null));
            entry = ((Array)item)[1];
            Assert.IsInstanceOfType(entry, typeof(Boolean));
            Assert.AreEqual(true, entry.GetBoolean());
            entry = ((Array)item)[2];
            Assert.IsInstanceOfType(entry, typeof(VM.Types.ByteString));
            Assert.AreEqual("asd", entry.GetString());
        }
    }
}
