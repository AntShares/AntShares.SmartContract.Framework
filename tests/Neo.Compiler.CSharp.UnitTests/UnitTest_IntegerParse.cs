using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_IntegerParse
    {
        [TestMethod]
        public void SByteParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testSbyteparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "127");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(127, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-128");

            value = result.Pop().GetInteger();
            Assert.AreEqual(-128, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "128");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-129");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void ByteParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testByteparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "0");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "255");

            value = result.Pop().GetInteger();
            Assert.AreEqual(255, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-1");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "256");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void UShortParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testUshortparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "0");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "65535");

            value = result.Pop().GetInteger();
            Assert.AreEqual(65535, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-1");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "65536");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void ShortParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testShortparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "-32768");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(-32768, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "32767");

            value = result.Pop().GetInteger();
            Assert.AreEqual(32767, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-32769");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "32768");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void ULongParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testUlongparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "18446744073709551615");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(18446744073709551615, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "0");

            value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "18446744073709551616");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-1");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void LongParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testLongparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, " -9223372036854775808");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(-9223372036854775808, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "9223372036854775807");

            value = result.Pop().GetInteger();
            Assert.AreEqual(9223372036854775807, value);

            //test backspace trip
            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);


            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-9223372036854775809");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "9223372036854775808");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void UIntParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testUintparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "4294967295");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(4294967295, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "0");

            value = result.Pop().GetInteger();
            Assert.AreEqual(0, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-1");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "4294967296");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }

        [TestMethod]
        public void IntParse_Test()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_IntegerParse.cs");
            string methodname = "testIntparse";

            var result = testengine.ExecuteTestCaseStandard(methodname, "2147483647");
            var value = result.Pop().GetInteger();
            Assert.AreEqual(2147483647, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-2147483648");

            value = result.Pop().GetInteger();
            Assert.AreEqual(-2147483648, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, " 20 ");

            value = result.Pop().GetInteger();
            Assert.AreEqual(20, value);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "2147483648");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "-2147483649");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "abc");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);

            testengine.Reset();
            result = testengine.ExecuteTestCaseStandard(methodname, "@");

            Assert.AreEqual(VMState.FAULT, testengine.State);
            Assert.IsNotNull(testengine.FaultException);
        }
    }
}
