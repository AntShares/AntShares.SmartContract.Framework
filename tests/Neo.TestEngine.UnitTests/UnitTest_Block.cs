using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.IO.Json;
using Neo.SmartContract.Native;
using Neo.TestEngine.UnitTests.Utils;
using Neo.TestingEngine;
using Neo.VM;
using Neo.VM.Types;
using System.IO;

namespace Neo.TestEngine.UnitTests
{
    [TestClass]
    public class UnitTest_Block
    {
        [TestInitialize]
        public void Init()
        {
            string path = Directory.GetCurrentDirectory();

            CSharpCompiler.Compile(path + "/TestClasses/Contract1.cs");
            CSharpCompiler.Compile(path + "/TestClasses/Contract_Time.cs");
            Engine.Instance.Reset();
        }

        [TestMethod]
        public void Test_Block_Height()
        {
            uint height = 16;

            var json = new JObject();
            json["path"] = "./TestClasses/Contract1.nef";
            json["method"] = "testVoid";
            json["height"] = height;

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

            Assert.AreEqual(height, Engine.Instance.Height);
        }

        [TestMethod]
        public void Test_Include_Block()
        {
            uint height = 10;
            ulong timestamp = TestBlockchain.TheNeoSystem.GenesisBlock.Timestamp + 20 * TestBlockchain.TheNeoSystem.Settings.MillisecondsPerBlock;

            var blockJson = new JObject();
            blockJson["index"] = height;
            blockJson["timestamp"] = timestamp;
            blockJson["transactions"] = new JArray();

            var json = new JObject();
            json["path"] = "./TestClasses/Contract_Time.nef";
            json["method"] = "getTime";
            json["blocks"] = new JArray() { blockJson };

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

            Assert.AreEqual(height, Engine.Instance.Height);

            // test result
            StackItem wantresult = timestamp;
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 1);
            Assert.IsTrue(resultStack[0].ContainsProperty("value"));
            Assert.AreEqual(resultStack[0]["value"].AsString(), wantresult.ToJson()["value"].AsString());
        }

        [TestMethod]
        public void Test_Include_Transaction()
        {
            var signer = new JObject();
            signer["account"] = "0x0000000000000000000000000000000000000000";
            signer["scopes"] = "None";

            var transactionJson = new JObject();
            transactionJson["script"] = "EMAMB2dldFRpbWUMFIsccf/5cagRfaIDVBBMkYOwR666QWJ9W1I=";
            transactionJson["witnesses"] = new JArray();
            transactionJson["signers"] = new JArray()
            {
                signer
            };

            uint height = 1;
            ulong timestamp = TestBlockchain.TheNeoSystem.GenesisBlock.Timestamp + 20 * TestBlockchain.TheNeoSystem.Settings.MillisecondsPerBlock;

            var blockJson = new JObject();
            blockJson["index"] = height;
            blockJson["timestamp"] = timestamp;
            blockJson["transactions"] = new JArray()
            {
                transactionJson
            };

            var json = new JObject();
            json["path"] = "./TestClasses/Contract_Time.nef";
            json["method"] = "getBlock";
            json["arguments"] = new JArray() { ((StackItem)height).ToParameter().ToJson() };
            json["blocks"] = new JArray() { blockJson };

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

            Assert.AreEqual(height, Engine.Instance.Height);

            // test result
            var block = NativeContract.Ledger.GetBlock(Engine.Instance.Snaptshot, Engine.Instance.Height);
            Assert.IsNotNull(block);
            Assert.AreEqual(block.Transactions.Length, ((JArray)blockJson["transactions"]).Count);
        }
    }
}
