using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.TestEngine.UnitTests.Utils;
using Neo.TestingEngine;
using Neo.VM;
using Neo.VM.Types;
using System.IO;

namespace Neo.TestEngine.UnitTests
{
    [TestClass]
    public class UnitTest_Notification
    {
        [TestInitialize]
        public void Init()
        {
            string path = Directory.GetCurrentDirectory();
            CSharpCompiler.Compile(path + "/TestClasses/Contract2.cs");
            Engine.Instance.Reset();
        }

        [TestMethod]
        public void Test_Notification()
        {
            StackItem arg1 = 16;
            StackItem arg2 = "Teste";

            var args = new string[] {
                "./TestClasses/Contract2.nef",
                "unitTest_002",
                arg1.ToParameter().ToJson().ToString(),
                arg2.ToParameter().ToJson().ToString()
            };
            var result = Program.Run(args);

            // mustn't have errors
            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNull(result["error"]);

            // test state
            Assert.IsTrue(result.ContainsProperty("vmstate"));
            Assert.AreEqual(result["vmstate"].AsString(), VMState.HALT.ToString());

            // test result
            StackItem wantresult = 3;
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 1);
            Assert.IsTrue(resultStack[0].ContainsProperty("value"));
            Assert.AreEqual(resultStack[0]["value"].AsString(), wantresult.ToJson()["value"].AsString());

            // test notifications
            Assert.IsTrue(result.ContainsProperty("notifications"));
            Assert.IsInstanceOfType(result["notifications"], typeof(JArray));

            var notifications = result["notifications"] as JArray;
            Assert.IsTrue(notifications.Count == 3);

            // emitted Deploy notification when the contract was deployed
            Assert.IsTrue(notifications[0].ContainsProperty("value"));
            Assert.IsTrue(notifications[0].ContainsProperty("eventname"));
            Assert.AreEqual(notifications[0]["eventname"].AsString(), "Deploy");

            Assert.IsTrue(notifications[1].ContainsProperty("value"));
            Assert.IsTrue(notifications[1].ContainsProperty("eventname"));
            Assert.AreEqual(notifications[1]["eventname"].AsString(), "event");
            Assert.IsTrue(notifications[1].ContainsProperty("value"));
            var firstNotifications = notifications[1]["value"];
            Assert.IsTrue(firstNotifications.ContainsProperty("value"));
            Assert.AreEqual((firstNotifications["value"] as JArray)[0].AsString(), arg1.ToJson().ToString());

            Assert.IsTrue(notifications[2].ContainsProperty("value"));
            Assert.IsTrue(notifications[2].ContainsProperty("eventname"));
            Assert.AreEqual(notifications[2]["eventname"].AsString(), "event");
            Assert.IsTrue(notifications[2].ContainsProperty("value"));
            var secondNotifications = notifications[2]["value"];
            Assert.IsTrue(secondNotifications.ContainsProperty("value"));
            Assert.AreEqual((secondNotifications["value"] as JArray)[0].AsString(), arg2.ToJson().ToString());
        }
    }
}
