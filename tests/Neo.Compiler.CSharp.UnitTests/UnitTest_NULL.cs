using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.SmartContract;
using Neo.SmartContract.Manifest;
using Neo.VM.Types;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_NULL
    {
        private TestEngine testengine;

        [TestInitialize]
        public void Init()
        {
            testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_NULL.cs");
        }

        [TestMethod]
        public void IsNull()
        {
            // True

            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("isNull", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            // False

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("isNull", new Integer(1));
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void IfNull()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("ifNull", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void NullProperty()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("nullProperty", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullProperty", "");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullProperty", "123");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());
        }

        [TestMethod]
        public void NullPropertyGT()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("nullPropertyGT", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyGT", "");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyGT", "123");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());
        }

        [TestMethod]
        public void NullPropertyLT()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("nullPropertyLT", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyLT", "");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyLT", "123");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void NullPropertyGE()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("nullPropertyGE", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyGE", "");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyGE", "123");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());
        }

        [TestMethod]
        public void nullPropertyLE()
        {
            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("nullPropertyLE", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyLE", "");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("nullPropertyLE", "123");
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void NullCoalescing()
        {
            //  call NullCoalescing(string code)
            // return  code ?.Substring(1,2);

            // a123b->12
            testengine.Reset();
            {
                var result = testengine.ExecuteTestCaseStandard("nullCoalescing", "a123b");
                var item = result.Pop().ConvertTo(StackItemType.ByteString) as ByteString;
                System.ReadOnlySpan<byte> data = item;
                var str = System.Text.Encoding.ASCII.GetString(data);
                Assert.IsTrue(str == "12");
            }
            // null->null

            testengine.Reset();
            {
                var result = testengine.ExecuteTestCaseStandard("nullCoalescing", StackItem.Null);
                var item = result.Pop();

                Assert.IsTrue(item.IsNull);
            }
        }

        [TestMethod]
        public void NullCollation()
        {
            // call nullCollation(string code)
            // return code ?? "linux"

            // nes->nes
            testengine.Reset();
            {
                var result = testengine.ExecuteTestCaseStandard("nullCollation", "nes");
                var item = result.Pop() as ByteString;
                System.ReadOnlySpan<byte> data = item;
                var str = System.Text.Encoding.ASCII.GetString(data);
                Assert.IsTrue(str == "nes");
            }

            // null->linux
            testengine.Reset();
            {
                var result = testengine.ExecuteTestCaseStandard("nullCollation", StackItem.Null);
                var item = result.Pop() as ByteString;
                System.ReadOnlySpan<byte> data = item;
                var str = System.Text.Encoding.ASCII.GetString(data);
                Assert.IsTrue(str == "linux");
            }
        }

        [TestMethod]
        public void NullCollationAndCollation()
        {
            var _testengine = new TestEngine(snapshot: new TestDataCache());
            _testengine.AddEntryScript("./TestClasses/Contract_NULL.cs");
            _testengine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = testengine.EntryScriptHash,
                Nef = testengine.Nef,
                Manifest = new ContractManifest()
            });

            var result = _testengine.ExecuteTestCaseStandard("nullCollationAndCollation", "nes");
            var item = result.Pop() as ByteString;
            Assert.AreEqual(123, item.GetSpan()[0]);
        }

        [TestMethod]
        public void NullCollationAndCollation2()
        {
            var _testengine = new TestEngine(snapshot: new TestDataCache());
            _testengine.AddEntryScript("./TestClasses/Contract_NULL.cs");
            _testengine.Snapshot.ContractAdd(new ContractState()
            {
                Hash = testengine.EntryScriptHash,
                Nef = testengine.Nef,
                Manifest = new ContractManifest()
            });

            var result = _testengine.ExecuteTestCaseStandard("nullCollationAndCollation2", "nes");
            var item = result.Pop() as ByteString;
            var bts = System.Text.Encoding.ASCII.GetBytes("111");
            var num = new System.Numerics.BigInteger(bts);

            Assert.AreEqual(num, item.GetInteger());
        }

        [TestMethod]
        public void EqualNull()
        {
            // True

            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("equalNullA", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            // False

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNullA", new Integer(1));
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            // True

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNullB", StackItem.Null);
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            // False

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNullB", new Integer(1));
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());
        }

        [TestMethod]
        public void EqualNotNull()
        {
            // True

            testengine.Reset();
            var result = testengine.ExecuteTestCaseStandard("equalNotNullA", StackItem.Null);
            var item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            // False

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNotNullA", new Integer(1));
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());

            // True

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNotNullB", StackItem.Null);
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            // False

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard("equalNotNullB", new Integer(1));
            item = result.Pop();

            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());
        }
    }
}
