using Neo.Network.P2P.Payloads;
using Neo.SmartContract;
using Neo.VM;
using System;
using System.Collections.Generic;

namespace Neo.Compiler.MSIL.Utils
{
    class TestEngine : ApplicationEngine
    {
        public const int MaxStorageKeySize = 64;
        public const int MaxStorageValueSize = ushort.MaxValue;

        static IDictionary<string, BuildScript> scriptsAll = new Dictionary<string, BuildScript>();

        public readonly IDictionary<string, BuildScript> Scripts;

        public BuildScript ScriptEntry { get; private set; }

        public TestEngine(TriggerType trigger = TriggerType.Application, IVerifiable verificable = null)
            : base(trigger, verificable, new TestSnapshot(), 0, true)
        {
            Scripts = new Dictionary<string, BuildScript>();
        }

        public BuildScript Build(string filename)
        {
            if (scriptsAll.ContainsKey(filename) == false)
            {
                scriptsAll[filename] = NeonTestTool.BuildScript(filename);
            }

            return scriptsAll[filename];
        }

        public void AddEntryScript(string filename)
        {
            ScriptEntry = Build(filename);
            Reset();
        }

        public void Reset()
        {
            this.State = VMState.BREAK; // Required for allow to reuse the same TestEngine
            this.InvocationStack.Clear();
            this.LoadScript(ScriptEntry.finalNEF);
        }

        public class ContractMethod
        {
            readonly TestEngine engine;
            readonly string methodname;

            public ContractMethod(TestEngine engine, string methodname)
            {
                this.engine = engine;
                this.methodname = methodname;
            }

            public StackItem Run(params StackItem[] _params)
            {
                return this.engine.ExecuteTestCaseStandard(methodname, _params).Pop();
            }
        }

        public ContractMethod GetMethod(string methodname)
        {
            return new ContractMethod(this, methodname);
        }

        public RandomAccessStack<StackItem> ExecuteTestCaseStandard(string methodname, params StackItem[] args)
        {
            //var engine = new ExecutionEngine();
            this.InvocationStack.Peek().InstructionPointer = 0;
            this.CurrentContext.EvaluationStack.Push(args);
            this.CurrentContext.EvaluationStack.Push(methodname);
            while (true)
            {
                var bfault = (this.State & VMState.FAULT) > 0;
                var bhalt = (this.State & VMState.HALT) > 0;
                if (bfault || bhalt) break;

                Console.WriteLine("op:[" +
                    this.CurrentContext.InstructionPointer.ToString("X04") +
                    "]" +
                this.CurrentContext.CurrentInstruction.OpCode);
                this.ExecuteNext();
            }
            return this.ResultStack;
        }

        public RandomAccessStack<StackItem> ExecuteTestCase(params StackItem[] args)
        {
            //var engine = new ExecutionEngine();
            this.InvocationStack.Peek().InstructionPointer = 0;
            if (args != null)
            {
                for (var i = args.Length - 1; i >= 0; i--)
                {
                    this.CurrentContext.EvaluationStack.Push(args[i]);
                }
            }
            while (true)
            {
                var bfault = (this.State & VMState.FAULT) > 0;
                var bhalt = (this.State & VMState.HALT) > 0;
                if (bfault || bhalt) break;

                Console.WriteLine("op:[" +
                    this.CurrentContext.InstructionPointer.ToString("X04") +
                    "]" +
                this.CurrentContext.CurrentInstruction.OpCode);
                this.ExecuteNext();
            }
            var stack = this.ResultStack;
            return stack;
        }

        protected override bool OnSysCall(uint method)
        {
            if (
               // Account
               method == InteropService.Neo_Account_IsStandard ||
               // Storages
               method == InteropService.System_Storage_GetContext ||
               method == InteropService.System_Storage_GetReadOnlyContext ||
               method == InteropService.System_Storage_Get ||
               method == InteropService.System_Storage_Delete ||
               method == InteropService.System_Storage_Put ||
               // ExecutionEngine
               method == InteropService.System_ExecutionEngine_GetCallingScriptHash ||
               method == InteropService.System_ExecutionEngine_GetEntryScriptHash ||
               method == InteropService.System_ExecutionEngine_GetExecutingScriptHash ||
               method == InteropService.System_ExecutionEngine_GetScriptContainer ||
               // Runtime
               method == InteropService.System_Runtime_CheckWitness ||
               method == InteropService.System_Runtime_GetNotifications ||
               method == InteropService.System_Runtime_GetInvocationCounter ||
               method == InteropService.System_Runtime_GetTrigger ||
               method == InteropService.System_Runtime_GetTime ||
               method == InteropService.System_Runtime_Platform ||
               method == InteropService.System_Runtime_Log ||
               method == InteropService.System_Runtime_Notify ||
               // Json
               method == InteropService.Neo_Json_Deserialize ||
               method == InteropService.Neo_Json_Serialize ||
               // Contract
               method == InteropService.System_Contract_Call
               )
            {
                return base.OnSysCall(method);
            }

            throw new Exception($"Syscall not found: {method.ToString("X2")} (using base call)");
        }

        public bool CheckAsciiChar(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                char c = s.ToLower().ToCharArray()[i];
                if ((!((c >= 97 && c <= 123) || (c >= 48 && c <= 57) || c == 45 || c == 46 || c == 64 || c == 95)))
                    return false;
            }
            return true;
        }

        private void DumpItemShort(StackItem item, int space = 0)
        {
            var spacestr = "";
            for (var i = 0; i < space; i++) spacestr += "    ";
            var line = NeonTestTool.Bytes2HexString(item.GetByteArray());

            if (item is Neo.VM.Types.ByteArray)
            {
                var str = item.GetString();
                if (CheckAsciiChar(str))
                {
                    line += "|" + str;
                }
            }
            Console.WriteLine(spacestr + line);
        }

        private void DumpItem(StackItem item, int space = 0)
        {
            var spacestr = "";
            for (var i = 0; i < space; i++) spacestr += "    ";
            Console.WriteLine(spacestr + "got Param:" + item.GetType().ToString());

            if (item is Neo.VM.Types.Array || item is Neo.VM.Types.Struct)
            {
                var array = item as Neo.VM.Types.Array;
                for (var i = 0; i < array.Count; i++)
                {
                    var subitem = array[i];
                    DumpItem(subitem, space + 1);
                }
            }
            else if (item is Neo.VM.Types.Map)
            {
                var map = item as Neo.VM.Types.Map;
                foreach (var subitem in map)
                {
                    Console.WriteLine("---Key---");
                    DumpItemShort(subitem.Key, space + 1);
                    Console.WriteLine("---Value---");
                    DumpItem(subitem.Value, space + 1);
                }
            }
            else
            {
                Console.WriteLine(spacestr + "--as num:" + item.GetBigInteger());

                Console.WriteLine(spacestr + "--as bin:" + NeonTestTool.Bytes2HexString(item.GetByteArray()));
                if (item is Neo.VM.Types.ByteArray)
                {
                    var str = item.GetString();
                    if (CheckAsciiChar(str))
                    {
                        Console.WriteLine(spacestr + "--as str:" + item.GetString());
                    }
                }
            }
        }
    }
}
