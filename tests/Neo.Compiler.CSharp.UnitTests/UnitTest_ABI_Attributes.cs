using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_ABI_Attributes
    {
        [TestMethod]
        public void TestAbiAttributes()
        {
            var testEngine = new TestEngine();
            testEngine.AddEntryScript("./TestClasses/Contract_ABIAttributes.cs");
            var permissions = testEngine.Manifest["permissions"].ToString(false);
            Assert.AreEqual(@"[{""contract"":""0x01ff00ff00ff00ff00ff00ff00ff00ff00ff00a4"",""methods"":[""a"",""b""]},{""contract"":""*"",""methods"":[""c""]}]", permissions);
            var trust = testEngine.Manifest["trusts"].ToString(false);
            Assert.AreEqual(@"[""0x0a0b00ff00ff00ff00ff00ff00ff00ff00ff00a4""]", trust);
        }
    }
}
