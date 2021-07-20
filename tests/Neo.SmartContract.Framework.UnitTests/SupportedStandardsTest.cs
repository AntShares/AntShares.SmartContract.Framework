using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.SmartContract.Framework.UnitTests
{
    [TestClass]
    public class SupportedStandardsTest
    {
        [TestMethod]
        public void TestAttribute()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_SupportedStandards.cs");
            Assert.AreEqual(testengine.Manifest["supportedstandards"].ToString(), @"[""NEP10"",""NEP5""]");
        }
    }
}
