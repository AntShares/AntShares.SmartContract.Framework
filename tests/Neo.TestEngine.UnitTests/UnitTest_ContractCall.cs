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
    public class UnitTest_ContractCall
    {
        [TestInitialize]
        public void Init()
        {
            string path = Directory.GetCurrentDirectory();
            CSharpCompiler.Compile(path + "/TestClasses/Contract_ContractCall.cs");
            Engine.Instance.Reset();
        }

        [TestMethod]
        public void Test_Json()
        {
            var contract = new JObject();
            contract["nef"] = "./TestClasses/Contract1.nef";

            var json = new JObject();
            json["path"] = "./TestClasses/Contract_ContractCall.nef";
            json["method"] = "testContractCall";
            json["contracts"] = new JArray()
            {
                contract
            };

            var args = new string[] {
                json.AsString()
            };
            var result = Program.Run(args);

            // mustn't have errors
            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNull(result["error"]);

            // test state
            Assert.IsTrue(result.ContainsProperty("vmstate"));
            Assert.AreEqual(result["vmstate"].AsString(), VMState.HALT.ToString());

            // test result
            StackItem wantresult = new byte[] { 1, 2, 3, 4 };
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 1);
            Assert.IsTrue(resultStack[0].ContainsProperty("value"));
            Assert.AreEqual(resultStack[0]["value"].AsString(), wantresult.ToJson()["value"].AsString());
        }

        [TestMethod]
        public void Test_ContractCall_Void()
        {
            var contract = new JObject();
            contract["nef"] = "./TestClasses/Contract1.nef";

            var json = new JObject();
            json["path"] = "./TestClasses/Contract_ContractCall.nef";
            json["method"] = "testContractCallVoid";
            json["contracts"] = new JArray()
            {
                contract
            };

            var args = new string[] {
                json.AsString()
            };
            var result = Program.Run(args);

            // mustn't have errors
            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNull(result["error"]);

            // test state
            Assert.IsTrue(result.ContainsProperty("vmstate"));
            Assert.AreEqual(result["vmstate"].AsString(), VMState.HALT.ToString());

            // test result
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.AreEqual(0, resultStack.Count);
        }
    }
}
