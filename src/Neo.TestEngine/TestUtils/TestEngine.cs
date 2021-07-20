using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Neo.Compiler;
using Neo.IO.Json;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Neo.TestingEngine
{
    public class TestEngine : ApplicationEngine
    {
        public const long TestGas = 2000_00000000;

        private static readonly List<MetadataReference> references = new();

        private long previousGasConsumed = 0;
        public long GasConsumedByLastExecution => GasConsumed - previousGasConsumed;

        public NefFile Nef { get; private set; }
        public JObject Manifest { get; private set; }
        public JObject DebugInfo { get; private set; }

        internal BuildScript ScriptContext { get; set; }

        public void ClearNotifications()
        {
            typeof(ApplicationEngine).GetField("notifications", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(this, null);
        }

        static TestEngine()
        {
            try
            {
                string coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
                references.Add(MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.dll")));
                references.Add(MetadataReference.CreateFromFile(Path.Combine(coreDir, "System.Runtime.InteropServices.dll")));
                references.Add(MetadataReference.CreateFromFile(typeof(string).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(DisplayNameAttribute).Assembly.Location));
                references.Add(MetadataReference.CreateFromFile(typeof(BigInteger).Assembly.Location));

                string folder = Path.GetFullPath("../../../../../src/Neo.SmartContract.Framework/");
                string obj = Path.Combine(folder, "obj");
                IEnumerable<SyntaxTree> st = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories)
                    .Where(p => !p.StartsWith(obj))
                    .OrderBy(p => p)
                    .Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p), path: p));
                CSharpCompilationOptions options = new(OutputKind.DynamicallyLinkedLibrary);
                CSharpCompilation cr = CSharpCompilation.Create(null, st, references, options);
                references.Add(cr.ToMetadataReference());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public TestEngine(TriggerType trigger = TriggerType.Application, IVerifiable verificable = null, DataCache snapshot = null, Block persistingBlock = null)
             : base(trigger, verificable, snapshot, persistingBlock, ProtocolSettings.Default, TestGas)
        {
        }

        public CompilationContext AddEntryScript(params string[] files)
        {
            ScriptContext = BuildScript.Build(references, files);
            SetContext(ScriptContext);

            return ScriptContext.Context;
        }

        public CompilationContext SetContext(BuildScript context)
        {
            ScriptContext = context;

            if (context.Success)
            {
                Nef = context.Nef;
                Manifest = context.Manifest;
                DebugInfo = context.DebugInfo;
                Reset();
            }

            return ScriptContext.Context;
        }

        public bool AddEntryScript(UInt160 contractHash)
        {
            var nativeContract = NativeContract.GetContract(contractHash);
            if (nativeContract != null)
            {
                return AddEntryScript(new BuildNative(nativeContract));
            }

            if (!Snapshot.ContainsContract(contractHash))
            {
                return false;
            }

            var state = NativeContract.ContractManagement.GetContract(Snapshot, contractHash);

            Nef = state.Nef;
            Manifest = state.Manifest.ToJson();
            Reset();

            return true;
        }

        public bool AddEntryScript(BuildScript script)
        {
            var contractHash = script.Nef.Script.ToScriptHash();
            var contract = NativeContract.ContractManagement.GetContract(Snapshot, contractHash);

            if (contract != null)
            {
                Nef = contract.Nef;
                Manifest = contract.Manifest.ToJson();
            }
            else
            {
                Nef = script.Nef;
                Manifest = script.Manifest;
            }

            Reset();
            ScriptContext = script;
            return true;
        }

        public void RunNativeContract(byte[] script, string method, StackItem[] parameters, CallFlags flags = CallFlags.All)
        {
            var rvcount = GetMethodReturnCount(method);
            var contractScript = new TestScript(script);

            InvocationStack.Pop();
            var context = CreateContext(contractScript, rvcount, 0);
            LoadContext(context);

            var mockedNef = new TestNefFile(script);
            ExecuteTestCaseStandard(0, (ushort)rvcount, mockedNef, new StackItem[0]);
        }

        public void Reset()
        {
            this.State = VMState.BREAK; // Required for allow to reuse the same TestEngine
            this.InvocationStack.Clear();
            while (this.ResultStack.Count > 0)
            {
                this.ResultStack.Pop();
            }
            if (Nef != null)
            {
                this.LoadScript(Nef.Script);
                // Mock contract
                var contextState = CurrentContext.GetState<ExecutionContextState>();
                contextState.Contract ??= new ContractState { Nef = Nef };
            }
        }

        private int GetMethodEntryOffset(string methodname)
        {
            if (Manifest is null) return -1;
            var methods = Manifest["abi"]["methods"] as JArray;
            foreach (var item in methods)
            {
                var method = item as JObject;
                if (method["name"].AsString() == methodname)
                    return int.Parse(method["offset"].AsString());
            }
            return -1;
        }

        private int GetMethodReturnCount(string methodname)
        {
            if (Manifest is null) return -1;
            var methods = Manifest["abi"]["methods"] as JArray;
            foreach (var item in methods)
            {
                var method = item as JObject;
                if (method["name"].AsString() == methodname)
                {
                    var returntype = method["returntype"].AsString();
                    if (returntype == "Null" || returntype == "Void")
                        return 0;
                    else
                        return 1;
                }
            }
            return -1;
        }

        public EvaluationStack ExecuteTestCaseStandard(string methodname, params StackItem[] args)
        {
            return ExecuteTestCaseStandard(methodname, Nef, args);
        }

        public EvaluationStack ExecuteTestCaseStandard(string methodname, NefFile contract, params StackItem[] args)
        {
            var offset = GetMethodEntryOffset(methodname);
            if (offset == -1) throw new Exception("Can't find method : " + methodname);
            var rvcount = GetMethodReturnCount(methodname);
            if (rvcount == -1) throw new Exception("Can't find method return count : " + methodname);
            return ExecuteTestCaseStandard(offset, (ushort)rvcount, contract, args);
        }

        public EvaluationStack ExecuteTestCaseStandard(int offset, ushort rvcount, params StackItem[] args)
        {
            return ExecuteTestCaseStandard(offset, rvcount, Nef, args);
        }

        public EvaluationStack ExecuteTestCaseStandard(int offset, ushort rvcount, NefFile contract, params StackItem[] args)
        {
            previousGasConsumed = GasConsumed;
            var context = InvocationStack.Pop();
            context = CreateContext(context.Script, rvcount, offset);
            LoadContext(context);
            // Mock contract
            var contextState = CurrentContext.GetState<ExecutionContextState>();
            contextState.Contract ??= new ContractState() { Nef = contract };
            contextState.ScriptHash = ScriptContext.ScriptHash;
            for (var i = args.Length - 1; i >= 0; i--)
                this.Push(args[i]);
            var initializeOffset = GetMethodEntryOffset("_initialize");
            if (initializeOffset != -1)
            {
                LoadContext(CurrentContext.Clone(initializeOffset));
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
            return this.ResultStack;
        }
    }
}
