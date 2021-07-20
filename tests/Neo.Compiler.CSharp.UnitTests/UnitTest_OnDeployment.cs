using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.IO.Json;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_OnDeployment
    {
        [TestMethod]
        public void Test_OnDeployment1()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_OnDeployment1.cs");

            var methods = (testengine.Manifest["abi"]["methods"] as JArray);

            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual(methods[0]["name"].AsString(), "_deploy");
            Assert.AreEqual(methods[0]["offset"].AsString(), "0");
            Assert.AreEqual(methods[0]["returntype"].AsString(), "Void");

            var args = (methods[0]["parameters"] as JArray);

            Assert.AreEqual(2, args.Count);
            Assert.AreEqual(args[0]["name"].AsString(), "data");
            Assert.AreEqual(args[0]["type"].AsString(), "Any");
            Assert.AreEqual(args[1]["name"].AsString(), "update");
            Assert.AreEqual(args[1]["type"].AsString(), "Boolean");
        }

        [TestMethod]
        public void Test_OnDeployment2()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_OnDeployment1.cs");

            var methods = (testengine.Manifest["abi"]["methods"] as JArray);

            Assert.AreEqual(1, methods.Count);
            Assert.AreEqual(methods[0]["name"].AsString(), "_deploy");
            Assert.AreEqual(methods[0]["offset"].AsString(), "0");
            Assert.AreEqual(methods[0]["returntype"].AsString(), "Void");

            var args = (methods[0]["parameters"] as JArray);

            Assert.AreEqual(2, args.Count);
            Assert.AreEqual(args[0]["name"].AsString(), "data");
            Assert.AreEqual(args[0]["type"].AsString(), "Any");
            Assert.AreEqual(args[1]["name"].AsString(), "update");
            Assert.AreEqual(args[1]["type"].AsString(), "Boolean");
        }
    }
}
