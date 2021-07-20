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
    public class UnitTest_Invoke
    {
        [TestInitialize]
        public void Init()
        {
            string path = Directory.GetCurrentDirectory();
            CSharpCompiler.Compile(path + "/TestClasses/Contract1.cs");
            Engine.Instance.Reset();
        }

        [TestMethod]
        public void Test_Missing_Arguments()
        {
            var args = new string[] {
                "./TestClasses/Contract1.nef"
            };
            var result = Program.Run(args);

            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.AreEqual(
                "One or more arguments are missing\nExpected arguments: <nef path> <method name> <method arguments as json>",
                result["error"].AsString()
            );
        }

        [TestMethod]
        public void Test_Method_Without_Parameters_Void()
        {
            var args = new string[] {
                "./TestClasses/Contract1.nef",
                "testVoid"
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
            Assert.IsTrue(resultStack.Count == 0);
        }

        [TestMethod]
        public void Test_Method_Without_Parameters_With_Return()
        {
            var args = new string[] {
                "./TestClasses/Contract1.nef",
                "unitTest_001"
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
        public void Test_Method_With_Parameters()
        {
            StackItem arguments = 16;
            var args = new string[] {
                "./TestClasses/Contract1.nef",
                "testArgs1",
                arguments.ToParameter().ToJson().ToString()
            };
            var result = Program.Run(args);

            // mustn't have errors
            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNull(result["error"]);

            // test state
            Assert.IsTrue(result.ContainsProperty("vmstate"));
            Assert.AreEqual(result["vmstate"].AsString(), VMState.HALT.ToString());

            // test result
            StackItem wantresult = new byte[] { 1, 2, 3, 16 };
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 1);
            Assert.IsTrue(resultStack[0].ContainsProperty("value"));
            Assert.AreEqual(resultStack[0]["value"].AsString(), wantresult.ToJson()["value"].AsString());
        }

        [TestMethod]
        public void Test_Method_With_Misstyped_Parameters()
        {
            var args = new string[] {
                "./TestClasses/Contract1.nef",
                "testArgs1"
            };
            var result = Program.Run(args);

            // mustn't have an error
            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNotNull(result["error"]);

            // vm state must've faulted
            Assert.IsTrue(result.ContainsProperty("vmstate"));
            Assert.AreEqual(result["vmstate"].AsString(), VMState.FAULT.ToString());

            // result stack must be empty
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 0);
        }

        [TestMethod]
        public void Test_Method_With_Parameters_Missing()
        {
            StackItem arguments = 16;
            var jsonArgument = arguments.ToParameter().ToJson().ToString();
            var args = new string[] {
                "./TestClasses/Contract1.nef",
                "testArgs1",
                $"{jsonArgument} {jsonArgument}"
            };
            var result = Program.Run(args);

            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.IsNotNull(result["error"]);
            Assert.AreEqual(result.Properties.Count, 1);
        }

        [TestMethod]
        public void Test_File_Does_Not_Exist()
        {
            var args = new string[] {
                "./TestClasses/Contract0.nef",
                "testArgs1",
            };
            var result = Program.Run(args);

            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.AreEqual(result["error"].AsString(), "File doesn't exists");
        }

        [TestMethod]
        public void Test_Invalid_File()
        {
            var args = new string[] {
                "./TestClasses/Contract1.cs",
                "testArgs1",
            };
            var result = Program.Run(args);

            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.AreEqual(result["error"].AsString(), "Invalid file. A .nef file required.");
        }

        [TestMethod]
        public void Test_Json_Missing_Fields()
        {
            var json = new JObject();
            json["path"] = "./TestClasses/Contract1.nef";

            var args = new string[] {
                json.AsString()
            };
            var result = Program.Run(args);

            Assert.IsTrue(result.ContainsProperty("error"));
            Assert.AreEqual("Missing field: 'method'", result["error"].AsString());
        }

        [TestMethod]
        public void Test_Json()
        {
            var json = new JObject();
            json["path"] = "./TestClasses/Contract1.nef";
            json["method"] = "unitTest_001";

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
        public void Test_Json_With_Parameters()
        {
            StackItem arguments = 16;

            var json = new JObject();
            json["path"] = "./TestClasses/Contract1.nef";
            json["method"] = "testArgs1";
            json["arguments"] = new JArray() { arguments.ToParameter().ToJson() };

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
            StackItem wantresult = new byte[] { 1, 2, 3, 16 };
            Assert.IsTrue(result.ContainsProperty("resultstack"));
            Assert.IsInstanceOfType(result["resultstack"], typeof(JArray));

            var resultStack = result["resultstack"] as JArray;
            Assert.IsTrue(resultStack.Count == 1);
            Assert.IsTrue(resultStack[0].ContainsProperty("value"));
            Assert.AreEqual(resultStack[0]["value"].AsString(), wantresult.ToJson()["value"].AsString());
        }

        [TestMethod]
        public void Test_Json_With_Storage()
        {
            PrimitiveType key = "example";
            key = new ByteString(key.GetSpan().ToArray());
            var storageKey = new JObject();
            storageKey["id"] = 0;
            storageKey["key"] = key.ToParameter().ToJson();

            StackItem value = "123";
            value = new ByteString(value.GetSpan().ToArray());
            var storageValue = new JObject();
            storageValue["isconstant"] = false;
            storageValue["value"] = value.ToParameter().ToJson();

            var storageItem = new JObject();
            storageItem["key"] = storageKey;
            storageItem["value"] = storageValue;

            StackItem arguments = 16;
            var json = new JObject();
            json["path"] = "./TestClasses/Contract1.nef";
            json["method"] = "testArgs1";
            json["arguments"] = new JArray() { arguments.ToParameter().ToJson() };
            json["storage"] = new JArray() { storageItem };

            var args = new string[] {
                json.AsString()
            };
            var result = Program.Run(args);

            // search in the storage
            Assert.IsTrue(result.ContainsProperty("storage"));
            Assert.IsInstanceOfType(result["storage"], typeof(JArray));

            storageKey["key"] = key.ToJson();
            storageValue["value"] = value.ToJson();
            var storageArray = result["storage"] as JArray;

            var contains = false;
            foreach (var pair in storageArray)
            {
                if (pair.AsString() == storageItem.AsString())
                {
                    contains = true;
                    break;
                }
            }
            Assert.IsTrue(contains);
        }
    }
}
