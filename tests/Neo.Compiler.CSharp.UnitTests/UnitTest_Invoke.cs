using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM.Types;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Invoke
    {
        private TestEngine testengine;

        [TestInitialize]
        public void Init()
        {
            testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_InvokeCsNef.cs");
        }

        [TestMethod]
        public void Test_Return_Integer()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("returnInteger");

            Integer wantresult = 42;
            Assert.IsTrue(wantresult.Equals(result.Pop()));
        }

        [TestMethod]
        public void Test_Return_String()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("returnString");

            ByteString wantresult = "hello world";
            Assert.IsTrue(wantresult.Equals(result.Pop()));
        }

        [TestMethod]
        public void Test_Main()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("main");

            Integer wantresult = 22;
            Assert.IsTrue(wantresult.Equals(result.Pop()));
        }
    }
}
