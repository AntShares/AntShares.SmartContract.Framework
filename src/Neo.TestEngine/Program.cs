using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Neo.TestingEngine
{
    public class Program
    {
        static int Main(string[] args)
        {
            JObject result = Run(args);
            Console.WriteLine(result);
            if (!result.ContainsProperty("vmstate"))
            {
                return -1;
            }
            return 0;
        }

        public static JObject Run(string[] args)
        {
            if (args.Length == 1)
            {
                var isFile = Path.GetExtension(args[0]).ToLowerInvariant() == ".json";
                JObject input;
                try
                {
                    // verifies if the parameter is a json string
                    string jsonString;
                    if (isFile)
                    {
                        jsonString = File.ReadAllText(args[0]);
                    }
                    else
                    {
                        jsonString = args[0];
                    }
                    input = JObject.Parse(jsonString);
                }
                catch
                {
                    // if it isn't, at least one argument is missing
                    string lastExpectedArg;
                    if (isFile)
                    {
                        lastExpectedArg = "json file with method arguments";
                    }
                    else
                    {
                        lastExpectedArg = "method arguments as json";
                    }

                    return BuildJsonException(
                        string.Format("One or more arguments are missing\n" +
                                      "Expected arguments: <nef path> <method name> <{0}>",
                                      lastExpectedArg)
                    );
                }

                return RunWithJson(input);
            }

            JObject result;
            if (args.Length >= 2)
            {
                string arg2 = "";
                if (args.Length == 3)
                {
                    arg2 = args[2];
                }
                else if (args.Length > 3)
                {
                    arg2 = $"[{string.Join(",", args.Skip(2))}]";
                }
                result = RunWithMethodName(args[0], args[1], arg2);
            }
            else
            {
                result = BuildJsonException(
                    "One or more arguments are missing\n" +
                    "Expected arguments: <nef path> <method name> <method arguments as json>"
                );
            }

            return result;
        }

        /// <summary>
        /// Runs a nef script given a method name and its arguments
        /// </summary>
        /// <param name="path">Absolute path of the script</param>
        /// <param name="methodName">The name of the targeted method</param>
        /// <param name="jsonParams">Json string representing the arguments of the method</param>
        /// <returns>Returns a json with the engine state after executing the script</returns>
        public static JObject RunWithMethodName(string path, string methodName, string jsonParams)
        {
            try
            {
                JArray parameters = new JArray();
                if (jsonParams.Length > 0)
                {
                    var json = JObject.Parse(jsonParams);
                    if (json is JArray array)
                    {
                        parameters = array;
                    }
                    else
                    {
                        parameters.Insert(0, json);
                    }
                }

                var smartContractTestCase = new SmartContractTest(path, methodName, parameters);
                return Run(smartContractTestCase);
            }
            catch (Exception e)
            {
                return BuildJsonException(e.Message);
            }
        }

        /// <summary>
        /// Runs a nef script given a json with test engine fake values for testing.
        /// </summary>
        /// <param name="json">json object with fake values and execution arguments</param>
        /// <returns>Returns a json with the engine state after executing the script</returns>
        public static JObject RunWithJson(JObject json)
        {
            var missigFieldMessage = "Missing field: '{0}'";
            if (!json.ContainsProperty("path") && !json.ContainsProperty("scripthash"))
            {
                return BuildJsonException(string.Format(missigFieldMessage, "path"));
            }

            if (!json.ContainsProperty("method"))
            {
                return BuildJsonException(string.Format(missigFieldMessage, "method"));
            }

            if (!json.ContainsProperty("arguments"))
            {
                json["arguments"] = new JArray();
            }

            try
            {
                var methodName = json["method"].AsString();
                var parameters = (JArray)json["arguments"];

                SmartContractTest smartContractTestCase;
                if (json.ContainsProperty("path") && json["path"].AsString().Length > 0)
                {
                    var path = json["path"].AsString();
                    smartContractTestCase = new SmartContractTest(path, methodName, parameters);
                }
                else
                {
                    var scriptHash = UInt160.Parse(json["scripthash"].AsString());
                    smartContractTestCase = new SmartContractTest(scriptHash, methodName, parameters);
                }

                // fake storage
                if (json.ContainsProperty("storage"))
                {
                    smartContractTestCase.storage = GetStorageFromJson(json["storage"]);
                }

                // additional contracts that aren't the entry point but can be called during executing
                if (json.ContainsProperty("contracts"))
                {
                    smartContractTestCase.contracts = GetContractsFromJson(json["contracts"]);
                }

                // set data for previous blocks for better simulation
                if (json.ContainsProperty("blocks") && json["blocks"] is JArray blocks)
                {
                    smartContractTestCase.blocks = blocks.Select(b => BlockFromJson(b)).ToArray();
                }

                // set the current heigh
                if (json.ContainsProperty("height"))
                {
                    smartContractTestCase.currentHeight = uint.Parse(json["height"].AsString());
                }

                // set data for current tx
                if (json.ContainsProperty("currenttx"))
                {
                    smartContractTestCase.currentTx = TxFromJson(json["currenttx"]);
                }

                // tx signers
                if (json.ContainsProperty("signeraccounts") && json["signeraccounts"] is JArray accounts)
                {
                    smartContractTestCase.signers = accounts.Select(p => UInt160.Parse(p.AsString())).ToArray();
                }
                return Run(smartContractTestCase);
            }
            catch (Exception e)
            {
                return BuildJsonException(e.Message);
            }
        }

        /// <summary>
        /// Runs the given method from a nef script
        /// </summary>
        /// <param name="smartContractTest">Object with the informations about the test case</param>
        /// <returns>Returns a json with the engine state after executing the script</returns>
        public static JObject Run(SmartContractTest smartContractTest)
        {
            try
            {
                if (smartContractTest.storage.Count > 0)
                {
                    Engine.Instance.SetStorage(smartContractTest.storage);
                }

                foreach (var contract in smartContractTest.contracts)
                {
                    Engine.Instance.AddSmartContract(contract);
                }

                foreach (var block in smartContractTest.blocks.OrderBy(b => b.Index))
                {
                    Engine.Instance.AddBlock(block);
                }

                Engine.Instance.IncreaseBlockCount(smartContractTest.currentHeight);
                Engine.Instance.SetSigners(smartContractTest.signers);

                if (smartContractTest.currentTx != null)
                {
                    Engine.Instance.SetTxAttributes(smartContractTest.currentTx.Attributes);
                }

                if (smartContractTest.nefPath != null)
                {
                    IsValidNefPath(smartContractTest.nefPath);
                    Engine.Instance.SetEntryScript(smartContractTest.nefPath);
                }
                else
                {
                    Engine.Instance.SetEntryScript(smartContractTest.scriptHash);
                }

                var stackParams = GetStackItemParameters(smartContractTest.methodParameters);
                return Engine.Instance.Run(smartContractTest.methodName, stackParams);
            }
            catch (Exception e)
            {
                return BuildJsonException(e.Message);
            }
        }

        /// <summary>
        /// Converts the data in a json array to a dictionary of StackItem
        /// </summary>
        /// <param name="jsonStorage">json array with the map values to be converted</param>
        /// <returns>Returns the built dictionary</returns>
        private static Dictionary<StorageKey, StorageItem> GetStorageFromJson(JObject jsonStorage)
        {
            if (!(jsonStorage is JArray storage))
            {
                throw new Exception("Expecting an array object in 'storage'");
            }

            var missingFieldMessage = "Missing field '{0}'";
            var items = new Dictionary<StorageKey, StorageItem>();
            foreach (var pair in storage)
            {
                if (!pair.ContainsProperty("key"))
                {
                    throw new Exception(string.Format(missingFieldMessage, "key"));
                }
                var jsonKey = pair["key"];
                if (!jsonKey.ContainsProperty("id") || !jsonKey.ContainsProperty("key"))
                {
                    throw new Exception(string.Format(missingFieldMessage, "key"));
                }

                if (!pair.ContainsProperty("value"))
                {
                    throw new Exception(string.Format(missingFieldMessage, "value"));
                }
                var jsonValue = pair["value"];
                if (!jsonValue.ContainsProperty("isconstant") || !jsonValue.ContainsProperty("value"))
                {
                    throw new Exception(string.Format(missingFieldMessage, "value"));
                }

                var key = (PrimitiveType)ContractParameter.FromJson(jsonKey["key"]).ToStackItem();
                var value = ContractParameter.FromJson(jsonValue["value"]).ToStackItem();

                var storageKey = new StorageKey()
                {
                    Id = int.Parse(jsonKey["id"].AsString()),
                    Key = key.GetSpan().ToArray()
                };

                StorageItem storageItem = null;
                if (value != StackItem.Null)
                {
                    storageItem = new StorageItem(value.GetSpan().ToArray());
                }
                items[storageKey] = storageItem;
            }

            return items;
        }

        /// <summary>
        /// Converts the data in a json array to a list of test smart contracts
        /// </summary>
        /// <param name="jsonContracts">json array with the contracts' paths to be converted</param>
        /// <returns>Returns a list of smart contracts for test</returns>
        private static List<TestContract> GetContractsFromJson(JObject jsonContracts)
        {
            if (!(jsonContracts is JArray contracts))
            {
                throw new Exception("Expecting an array object in 'contracts'");
            }

            var items = new List<TestContract>();
            foreach (var pair in contracts)
            {
                if (!pair.ContainsProperty("nef"))
                {
                    throw new Exception("Missing field 'nef'");
                }

                var path = pair["nef"].AsString();
                IsValidNefPath(path);
                items.Add(new TestContract(path));
            }

            return items;
        }

        /// <summary>
        /// Converts the data in a json array to an array of StackItem
        /// </summary>
        /// <param name="parameters">json array to be converted</param>
        /// <returns>Returns the built StackItem array</returns>
        private static ContractParameter[] GetStackItemParameters(JArray parameters)
        {
            var items = new List<ContractParameter>();
            foreach (JObject param in parameters)
            {
                var success = false;
                if (param.ContainsProperty("value") && param.ContainsProperty("type"))
                {
                    try
                    {
                        items.Add(ContractParameter.FromJson(param));
                        success = true;
                    }
                    catch (Exception e)
                    {
                        // if something went wrong while reading the json, log the error
                        Console.WriteLine(e);
                    }
                }

                if (!success)
                {
                    // if something went wrong while reading the json, inserts null in this argument position
                    items.Add(new ContractParameter()
                    {
                        Type = ContractParameterType.Any,
                        Value = null
                    });
                }
            }
            return items.ToArray();
        }

        private static Block BlockFromJson(JObject blockJson)
        {
            var transactions = blockJson["transactions"] as JArray;
            return new Block()
            {
                Header = new Header()
                {
                    Index = uint.Parse(blockJson["index"].AsString()),
                    Timestamp = ulong.Parse(blockJson["timestamp"].AsString())
                },
                Transactions = transactions.Select(b => TxFromJson(b)).ToArray()
            };
        }

        private static Transaction TxFromJson(JObject txJson)
        {
            Signer[] accounts;
            Witness[] witnesses;
            List<TransactionAttribute> attributes = new List<TransactionAttribute>();

            if (txJson.ContainsProperty("signers") && txJson["signers"] is JArray signersJson)
            {
                accounts = signersJson.Select(p => new Signer()
                {
                    Account = UInt160.Parse(p["account"].AsString()),
                    Scopes = WitnessScope.CalledByEntry
                }).ToArray();
            }
            else
            {
                accounts = new Signer[0];
            }

            if (txJson.ContainsProperty("witnesses") && txJson["witnesses"] is JArray witnessesJson)
            {
                witnesses = witnessesJson.Select(w => new Witness()
                {
                    InvocationScript = Convert.FromBase64String(w["invocation"].AsString()),
                    VerificationScript = Convert.FromBase64String(w["verification"].AsString())
                }).ToArray();
            }
            else
            {
                witnesses = new Witness[0];
            }

            if (txJson.ContainsProperty("attributes") && txJson["attributes"] is JArray attributesJson)
            {
                foreach (var attr in attributesJson)
                {
                    var txAttribute = TxAttributeFromJson(attr);
                    if (txAttribute != null)
                    {
                        attributes.Add(txAttribute);
                    }
                }
            }

            byte[] script;
            if (txJson.ContainsProperty("script"))
            {
                script = Convert.FromBase64String(txJson["script"].AsString());
            }
            else if (attributes.Any(attribute => attribute is OracleResponse oracleAttr))
            {
                script = OracleResponse.FixedScript;
            }
            else
            {
                script = new byte[0];
            }

            return new Transaction()
            {
                Script = script,
                Signers = accounts,
                Witnesses = witnesses,
                Attributes = attributes.ToArray()
            };
        }

        private static TransactionAttribute TxAttributeFromJson(JObject txAttributeJson)
        {
            if (!txAttributeJson.ContainsProperty("type"))
            {
                return null;
            }

            if (!Enum.TryParse<TransactionAttributeType>(txAttributeJson["type"].AsString(), out var type))
            {
                return null;
            }

            switch (type)
            {
                case TransactionAttributeType.OracleResponse:

                    if (!Enum.TryParse<OracleResponseCode>(txAttributeJson["code"].AsString(), out var responseCode))
                    {
                        throw new ArgumentException();
                    }

                    return new OracleResponse()
                    {
                        Id = ulong.Parse(txAttributeJson["id"].AsString()),
                        Code = responseCode,
                        Result = Convert.FromBase64String(txAttributeJson["result"].AsString())
                    };
                case TransactionAttributeType.HighPriority:
                    return new HighPriorityAttribute();
                default:
                    return null;
            }
        }

        private static bool IsValidNefPath(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("File doesn't exists");
            }

            if (Path.GetExtension(path).ToLowerInvariant() != ".nef")
            {
                throw new Exception("Invalid file. A .nef file required.");
            }

            return true;
        }

        private static JObject BuildJsonException(string message)
        {
            var json = new JObject();
            json["error"] = message;
            return json;
        }
    }
}
