using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Instance
    {
        private TestEngine testengine;

        [TestInitialize]
        public void Init()
        {
            testengine = new TestEngine(snapshot: null);
            var ctx = testengine.AddEntryScript("./TestClasses/Contract_Instance.cs");
            Assert.AreEqual("this,Any,0", ctx.CreateDebugInformation()["methods"][0]["params"][0].AsString());
            Assert.AreEqual("this,Any,0", ctx.CreateDebugInformation()["methods"][1]["params"][0].AsString());
        }

        [TestMethod]
        public void TestFunc()
        {
            testengine.Reset();

            var result = testengine.ExecuteTestCaseStandard("sum", 2).Pop();
            Assert.AreEqual(3, result.GetInteger());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("sum", 3).Pop();
            Assert.AreEqual(4, result.GetInteger());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("sum2", 3).Pop();
            Assert.AreEqual(8, result.GetInteger());
        }
    }
}
