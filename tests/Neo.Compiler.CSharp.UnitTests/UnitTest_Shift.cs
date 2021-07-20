using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.SmartContract;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_Shift
    {
        [TestMethod]
        public void Test_Shift()
        {
            var list = new List<BigInteger>();
            var method = new EventHandler<NotifyEventArgs>((sender, e) => list.Add(((VM.Types.Integer)((VM.Types.Array)e.State)[0]).GetInteger()));
            ApplicationEngine.Notify += method;

            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_shift.cs");
            //testengine.ScriptEntry.DumpNEF();
            var result = testengine.ExecuteTestCaseStandard("main");
            ApplicationEngine.Notify -= method;

            CollectionAssert.AreEqual(new BigInteger[] { 16, 4 }, list);
        }

        [TestMethod]
        public void Test_Shift_BigInteger()
        {
            var list = new List<BigInteger>();
            var method = new EventHandler<NotifyEventArgs>((sender, e) => list.Add(((VM.Types.Integer)((VM.Types.Array)e.State)[0]).GetInteger()));
            ApplicationEngine.Notify += method;

            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_shift_bigint.cs");
            //testengine.ScriptEntry.DumpNEF();
            var result = testengine.ExecuteTestCaseStandard("main");
            ApplicationEngine.Notify -= method;

            CollectionAssert.AreEqual(new BigInteger[] { 8, 16, 4, 2 }, list);
        }
    }
}
