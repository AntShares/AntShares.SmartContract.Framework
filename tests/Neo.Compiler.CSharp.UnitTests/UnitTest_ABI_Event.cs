using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.IO.Json;
using System;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_ABI_Event
    {
        [TestMethod]
        public void Test_Good()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_Event.cs");
            var abi = testengine.Manifest["abi"];
            Console.WriteLine("abi=" + abi.ToString());
            var events = (abi["events"] as JArray)[0].ToString();
            Console.WriteLine("event abi info =" + events);

            string expecteventabi = @"{""name"":""transfer"",""parameters"":[{""name"":""arg1"",""type"":""ByteArray""},{""name"":""arg2"",""type"":""ByteArray""},{""name"":""arg3"",""type"":""Integer""}]}";
            Assert.AreEqual(expecteventabi, events);
        }

        [TestMethod]
        public void Test_Wrong()
        {
            var testengine = new TestEngine();
            Assert.IsFalse(testengine.AddEntryScript("./TestClasses/Contract_WrongEvent.cs").Success);
        }
    }
}
