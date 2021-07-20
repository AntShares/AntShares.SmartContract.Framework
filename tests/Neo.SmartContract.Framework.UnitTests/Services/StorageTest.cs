using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM.Types;
using System;
using System.Linq;
using static Neo.Helper;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class StorageTest
    {
        private static void Put(TestEngine testengine, string method, byte[] prefix, byte[] key, byte[] value)
        {
            var result = testengine.ExecuteTestCaseStandard(method, key, value);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            var rItem = result.Pop();

            Assert.IsInstanceOfType(rItem, typeof(VM.Types.Boolean));
            Assert.AreEqual(true, rItem.GetBoolean());
            Assert.AreEqual(1,
                testengine.Snapshot.GetChangeSet()
                .Count(a =>
                    a.Key.Key.SequenceEqual(Concat(prefix, key)) &&
                    a.Item.Value.SequenceEqual(value)));
        }

        private static byte[] Get(TestEngine testengine, string method, byte[] prefix, byte[] key)
        {
            var result = testengine.ExecuteTestCaseStandard(method, key);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);
            var rItem = result.Pop();
            Assert.IsInstanceOfType(rItem, typeof(VM.Types.Buffer));
            ReadOnlySpan<byte> data = rItem.GetSpan();
            Assert.AreEqual(1, testengine.Snapshot.GetChangeSet().Count(a => a.Key.Key.SequenceEqual(Concat(prefix, key))));
            return data.ToArray();
        }

        private static void Delete(TestEngine testengine, string method, byte[] prefix, byte[] key)
        {
            var result = testengine.ExecuteTestCaseStandard(method, new VM.Types.ByteString(key));
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(0, result.Count);
            Assert.AreEqual(0, testengine.Snapshot.GetChangeSet().Count(a => a.Key.Key.SequenceEqual(Concat(prefix, key))));
        }

        private TestEngine testengine;

        [TestInitialize]
        public void Init()
        {
            var system = TestBlockchain.TheNeoSystem;
            var snapshot = system.GetSnapshot();

            testengine = new TestEngine(snapshot: snapshot.CreateSnapshot());
            testengine.AddEntryScript("./TestClasses/Contract_Storage.cs");
            testengine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = testengine.EntryScriptHash,
                Nef = testengine.Nef,
                Manifest = new Manifest.ContractManifest()
            });
        }

        [TestMethod]
        public void Test_StorageMap()
        {
            testengine.Reset();
            var ret = testengine.ExecuteTestCaseStandard("serializeTest", new byte[] { 1, 2, 3 }, 456);
            Assert.AreEqual(1, ret.Count);
            Assert.AreEqual(456, ret.Pop().GetInteger());
        }

        [TestMethod]
        public void Test_Byte()
        {
            var prefix = new byte[] { 0x11 };
            var key = new byte[] { 0x01, 0x02, 0x03 };
            var value = new byte[] { 0x04, 0x05, 0x06 };

            // Put

            Put(testengine, "testPutByte", prefix, key, value);

            // Get

            testengine.Reset();
            var getVal = Get(testengine, "testGetByte", prefix, key);
            CollectionAssert.AreEqual(value, getVal);

            // Delete

            testengine.Reset();
            Delete(testengine, "testDeleteByte", prefix, key);
        }

        [TestMethod]
        public void Test_ByteArray()
        {
            var prefix = new byte[] { 0x00, 0xFF };
            var key = new byte[] { 0x01, 0x02, 0x03 };
            var value = new byte[] { 0x04, 0x05, 0x06 };

            // Put

            Put(testengine, "testPutByteArray", prefix, key, value);

            // Get

            testengine.Reset();
            var getVal = Get(testengine, "testGetByteArray", prefix, key);
            CollectionAssert.AreEqual(value, getVal);

            // Delete

            testengine.Reset();
            Delete(testengine, "testDeleteByteArray", prefix, key);
        }

        [TestMethod]
        public void Test_LongBytes()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("testOver16Bytes");
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(1, result.Count);

            var bs = result.Pop().GetSpan().ToArray();
            var value = new byte[] { 0x3b, 0x00, 0x32, 0x03, 0x23, 0x23, 0x23, 0x23, 0x02, 0x23, 0x23, 0x02, 0x23, 0x23, 0x02, 0x23, 0x23, 0x02, 0x23, 0x23, 0x02, 0x23, 0x23, 0x02 };

            CollectionAssert.AreEqual(value, bs);
        }

        [TestMethod]
        public void Test_String()
        {
            var prefix = new byte[] { 0x61, 0x61 };
            var key = new byte[] { 0x01, 0x02, 0x03 };
            var value = new byte[] { 0x04, 0x05, 0x06 };

            // Put

            testengine.Reset();
            Put(testengine, "testPutString", prefix, key, value);

            // Get

            testengine.Reset();
            var getVal = Get(testengine, "testGetString", prefix, key);
            CollectionAssert.AreEqual(value, getVal);

            // Delete

            testengine.Reset();
            Delete(testengine, "testDeleteString", prefix, key);
        }

        [TestMethod]
        public void Test_ReadOnly()
        {
            var key = new byte[] { 0x01, 0x02, 0x03 };
            var value = new byte[] { 0x04, 0x05, 0x06 };

            // Put

            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("testPutReadOnly", new VM.Types.ByteString(key), new VM.Types.ByteString(value));
            Assert.AreEqual(VM.VMState.FAULT, testengine.State);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void Test_Find()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("testFind");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(new ByteString(new byte[] { 0x01 }), result.Pop());
        }

        [TestMethod]
        public void Test_Index()
        {
            testengine.Reset();
            var value = "123";
            var result = testengine.ExecuteTestCaseStandard("testIndexPut", "key", value);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(true, result.Pop());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("testIndexGet", "key");
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            Assert.AreEqual(value, result.Pop().GetString());
        }
    }
}
