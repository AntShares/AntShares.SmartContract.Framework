using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Math
    {
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine();
            _engine.AddEntryScript("./TestClasses/Contract_Math.cs");
        }

        [TestMethod]
        public void max_test()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("max", 1, 2);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(2, result.Pop().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("max", 3, 1);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(3, result.Pop().GetInteger());
        }

        [TestMethod]
        public void min_test()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("min", 1, 2);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Pop().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("min", 3, 1);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Pop().GetInteger());
        }

        [TestMethod]
        public void sign_test()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("sign", 1);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Pop().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("sign", -1);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(-1, result.Pop().GetInteger());

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("sign", 0);

            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(0, result.Pop().GetInteger());
        }
    }
}
