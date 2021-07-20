using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Concat
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_Concat.cs");
        }

        [TestMethod]
        public void TestStringAdd1()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testStringAdd1", "a");

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual("ahello", result.Pop().GetString());
        }

        [TestMethod]
        public void TestStringAdd2()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testStringAdd2", "a", "b");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual("abhello", result.Pop().GetString());
        }

        [TestMethod]
        public void TestStringAdd3()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testStringAdd3", "a", "b", "c");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual("abchello", result.Pop().GetString());
        }

        [TestMethod]
        public void TestStringAdd4()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testStringAdd4", "a", "b", "c", "d");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual("abcdhello", result.Pop().GetString());
        }
    }
}
