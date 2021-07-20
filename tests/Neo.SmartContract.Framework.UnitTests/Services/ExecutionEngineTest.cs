using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.IO;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class ExecutionEngineTest
    {
        class DummyVerificable : IVerifiable, IInteroperable
        {
            public Witness[] Witnesses { get; set; }

            public int Size => 0;

            public void Deserialize(BinaryReader reader)
            {
            }

            public void DeserializeUnsigned(BinaryReader reader)
            {
            }

            public void FromStackItem(StackItem stackItem)
            {
                throw new NotImplementedException();
            }

            public UInt160[] GetScriptHashesForVerifying(DataCache snapshot)
            {
                throw new NotImplementedException();
            }

            public void Serialize(BinaryWriter writer)
            {
            }

            public void SerializeUnsigned(BinaryWriter writer)
            {
            }

            public StackItem ToStackItem(ReferenceCounter referenceCounter)
            {
                return new VM.Types.Array();
            }
        }

        private TestEngine _engine;
        private string scriptHash;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(TriggerType.Application, new DummyVerificable());
            _engine.AddEntryScript("./TestClasses/Contract_ExecutionEngine.cs");
            scriptHash = _engine.Nef.Script.ToScriptHash().ToArray().ToHexString();
        }

        [TestMethod]
        public void CallingScriptHashTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("callingScriptHash");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Null));
        }

        [TestMethod]
        public void EntryScriptHashTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("entryScriptHash");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Buffer));
            //test by this way is bad idea? how to sure got a fix hash always?
            var gothash = item.GetSpan().ToHexString();
            Assert.AreEqual(scriptHash, gothash);
        }

        [TestMethod]
        public void ExecutingScriptHashTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("executingScriptHash");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(VM.Types.Buffer));
            //test by this way is bad idea? how to sure got a fix hash always? 
            var gothash = item.GetSpan().ToHexString();
            Assert.AreEqual(scriptHash, gothash);
        }

        [TestMethod]
        public void ScriptContainerTest()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("scriptContainer");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop() as VM.Types.Array;
            Assert.AreEqual(0, item.Count);
        }
    }
}
