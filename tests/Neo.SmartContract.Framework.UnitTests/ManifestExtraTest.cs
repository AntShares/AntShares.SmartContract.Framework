using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.SmartContract.Framework.UnitTests
{
    [TestClass]
    public class ManifestExtraTest
    {
        [TestMethod]
        public void TestExtraAttribute()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ExtraAttribute.cs");

            var extra = testengine.Manifest["extra"];

            Assert.AreEqual("Neo", extra["Author"].GetString());
            Assert.AreEqual("dev@neo.org", extra["E-mail"].GetString());
        }
    }
}
