using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Extensions
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_Extensions.cs");
        }

        [TestMethod]
        public void TestSum()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("testSum", 3, 2);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(5, result.Pop().GetInteger());
        }
    }
}
