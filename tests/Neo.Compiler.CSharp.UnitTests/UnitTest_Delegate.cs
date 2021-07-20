using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Delegate
    {
        private TestEngine testengine;
        private TestDataCache snapshot;

        [TestInitialize]
        public void Init()
        {
            snapshot = new TestDataCache();
            testengine = new TestEngine(snapshot: snapshot);
            testengine.AddEntryScript("./TestClasses/Contract_Delegate.cs");
        }

        [TestMethod]
        public void TestFunc()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("sumFunc", 2, 3).Pop();
            Assert.AreEqual(5, result.GetInteger());
        }
    }
}
