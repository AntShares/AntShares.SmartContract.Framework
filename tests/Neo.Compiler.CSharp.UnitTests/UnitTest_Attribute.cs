using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Attribute
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_Attribute.cs");
        }

        [TestMethod]
        public void test()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("test");

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.IsTrue(result.Pop().GetBoolean());
        }
    }
}
