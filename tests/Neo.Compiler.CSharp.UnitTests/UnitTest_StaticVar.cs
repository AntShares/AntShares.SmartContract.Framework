using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.VM.Types;
using System;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_StaticVar
    {
        [TestMethod]
        public void Test_StaticVar()
        {

            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_StaticVar.cs");
            var result = testengine.ExecuteTestCaseStandard("main");

            //test (1+5)*7 == 42
            StackItem wantresult = 42;
            var bequal = wantresult.Equals(result.Pop());
            Assert.IsTrue(bequal);
        }

        [TestMethod]
        public void Test_StaticVarInit()
        {
            VM.Types.Buffer var1;
            ByteString var2;

            using var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_StaticVarInit.cs");
            var result = testengine.ExecuteTestCaseStandard("staticInit");
            // static byte[] callscript = ExecutionEngine.EntryScriptHash;
            // ...
            // return callscript
            var1 = result.Pop() as VM.Types.Buffer;

            testengine.Reset();
            testengine.AddEntryScript("./TestClasses/Contract_StaticVarInit.cs");
            result = testengine.ExecuteTestCaseStandard("directGet");
            // return ExecutionEngine.EntryScriptHash
            var2 = result.Pop() as ByteString;

            Assert.IsNotNull(var1);
            Assert.IsTrue(var1.GetSpan().SequenceEqual(var2.GetSpan()));
        }

        [TestMethod]
        public void Test_testBigIntegerParse()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_StaticVar.cs");
            var result = testengine.ExecuteTestCaseStandard("testBigIntegerParse");
            var var1 = result.Pop();
            Assert.IsInstanceOfType(var1, typeof(Integer));
            Assert.AreEqual(123, var1.GetInteger());
        }

        [TestMethod]
        public void Test_testBigIntegerParse2()
        {
            using var testengine = new TestEngine(snapshot: new TestDataCache());
            testengine.AddEntryScript("./TestClasses/Contract_StaticVar.cs");
            var result = testengine.ExecuteTestCaseStandard("testBigIntegerParse2", "123");
            var var1 = result.Pop();
            Assert.IsInstanceOfType(var1, typeof(Integer));
            Assert.AreEqual(123, var1.GetInteger());
        }

        [TestMethod]
        public void Test_StaticConsturct()
        {
            StackItem var1;
            try
            {
                using var testengine = new TestEngine();
                testengine.AddEntryScript("./TestClasses/Contract_StaticConstruct.cs");
                var result = testengine.ExecuteTestCaseStandard("testStatic");
                // static byte[] callscript = ExecutionEngine.EntryScriptHash;
                // ...
                // return callscript
                var1 = (result.Pop());

                Assert.IsNotNull(var1);
                Assert.IsTrue(var1.GetInteger() == 4);
                Assert.Fail("should throw a error \"not support opcode xxx\" in this case.");
            }
            catch (Exception err)
            {
                Console.WriteLine("error message:" + err.Message);
                //need throw a error.
                Assert.IsTrue(err.Message.Contains("not support opcode"));
            }
        }
    }
}
