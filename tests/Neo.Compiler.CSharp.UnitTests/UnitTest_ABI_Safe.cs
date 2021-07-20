using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_ABI_Safe
    {
        [TestMethod]
        public void UnitTest_TestSafe()
        {
            var testEngine = new TestEngine();
            testEngine.AddEntryScript("./TestClasses/Contract_ABISafe.cs");

            var methodsABI = testEngine.Manifest["abi"]["methods"] as JArray;
            Assert.IsFalse(methodsABI[0]["safe"].AsBoolean());
            Assert.IsTrue(methodsABI[1]["safe"].AsBoolean());
            Assert.IsFalse(methodsABI[2]["safe"].AsBoolean());
        }
    }
}
