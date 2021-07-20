using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_GoTo
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_GoTo.cs");
        }

        [TestMethod]
        public void test()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("test");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(3, result.Pop().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("testTry");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(3, result.Pop().GetInteger());
        }
    }
}
