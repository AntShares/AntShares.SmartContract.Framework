using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Polymorphism
    {
        private TestEngine testengine;

        [TestInitialize]
        public void Init()
        {
            testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_Polymorphism.cs");
        }

        [TestMethod]
        public void Test()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("sum", 5, 9);
            Assert.AreEqual(14, result.Pop().GetInteger());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("mul", 5, 8);
            Assert.AreEqual(40, result.Pop().GetInteger());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("test");
            Assert.AreEqual("test", result.Pop().GetString());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("test2");
            Assert.AreEqual("base.test", result.Pop().GetString());
        }
    }
}
