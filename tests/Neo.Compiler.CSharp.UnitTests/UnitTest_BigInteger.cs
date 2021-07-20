using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_BigInteger
    {
        [TestMethod]
        public void Test_Pow()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testPow", 2, 3);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(8, value);
        }

        [TestMethod]
        public void Test_Sqrt()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testSqrt", 4);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(2, value);
        }

        [TestMethod]
        public void Test_Sbyte()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testsbyte", 127);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(127, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testsbyte", -128);

            value = result.Pop().GetInteger();
            Assert.AreEqual(-128, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testsbyte", 128);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testsbyte", -129);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_byte()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testbyte", 0);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testbyte", 255);

            value = result.Pop().GetInteger();
            Assert.AreEqual(255, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testbyte", -1);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testbyte", 256);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

        }

        [TestMethod]
        public void Test_short()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testshort", 32767);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(32767, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testshort", -32768);

            value = result.Pop().GetInteger();
            Assert.AreEqual(-32768, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testshort", 32768);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testshort", -32769);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_ushort()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testushort", 0);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testushort", 65535);

            value = result.Pop().GetInteger();
            Assert.AreEqual(65535, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testushort", -1);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testushort", 65536);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_int()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testint", -2147483648);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(-2147483648, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testint", 2147483647);

            value = result.Pop().GetInteger();
            Assert.AreEqual(2147483647, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testint", -2147483649);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testint", 2147483648);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_uint()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testuint", 0);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testuint", 4294967295);

            value = result.Pop().GetInteger();
            Assert.AreEqual(4294967295, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testuint", -1);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testuint", 4294967296);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_long()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testlong", -9223372036854775808);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(-9223372036854775808, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testlong", 9223372036854775807);

            value = result.Pop().GetInteger();
            Assert.AreEqual(9223372036854775807, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testlong", 9223372036854775808);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void Test_ulong()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            var result = testengine.ExecuteTestCaseStandard("testulong", 0);

            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testulong", 18446744073709551615);

            value = result.Pop().GetInteger();
            Assert.AreEqual(18446744073709551615, value);

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_BigInteger.cs");
            result = testengine.ExecuteTestCaseStandard("testulong", -1);

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }
    }
}
