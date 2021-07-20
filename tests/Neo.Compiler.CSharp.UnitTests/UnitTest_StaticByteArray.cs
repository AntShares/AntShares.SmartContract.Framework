using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM.Types;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_StaticByteArray
    {
        [TestMethod]
        public void TestStaticByteArray()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_StaticByteArray.cs");
            var result = testengine.ExecuteTestCaseStandard("testStaticByteArray").Pop();
            var wantResult = new byte[] { 0x89, 0x77, 0x20, 0xd8, 0xcd, 0x76, 0xf4, 0xf0, 0x0a, 0xbf, 0xa3, 0x7c, 0x0e, 0xdd, 0x88, 0x9c, 0x20, 0x8f, 0xde, 0x9b };
            Assert.AreEqual(wantResult, result.ConvertTo(StackItemType.ByteString));
        }
    }
}
