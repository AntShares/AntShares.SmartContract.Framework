using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM.Types;

namespace Neo.SmartContract.Framework.UnitTests
{
    [TestClass]
    public class StringTest
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(snapshot: new TestDataCache());
            _engine.AddEntryScript("./TestClasses/Contract_String.cs");
        }

        [TestMethod]
        public void TestStringAdd()
        {
            // ab => 3
            var result = _engine.ExecuteTestCaseStandard("testStringAdd", "a", "b");
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(3, item);

            // hello => 4
            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("testStringAdd", "he", "llo");
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(4, item);

            // world => 5
            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("testStringAdd", "wo", "rld");
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Integer));
            Assert.AreEqual(5, item);
        }

        [TestMethod]
        public void TestStringAddInt()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testStringAddInt", "Neo", 3);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(ByteString));
            Assert.AreEqual("Neo3", item.GetString());
        }
    }
}
