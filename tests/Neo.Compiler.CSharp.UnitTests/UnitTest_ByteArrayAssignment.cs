using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;

namespace Neo.Compiler.CSharp.UnitTests
{
    [TestClass]
    public class UnitTest_ByteArrayAssignment
    {
        [TestMethod]
        public void Test_ByteArrayAssignment()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ByteArrayAssignment.cs");

            var result = testengine.ExecuteTestCaseStandard("testAssignment").Pop();
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x04 }, result.GetSpan().ToArray());
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentOutOfBounds()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ByteArrayAssignment.cs");

            testengine.ExecuteTestCaseStandard("testAssignmentOutOfBounds");
            Assert.AreEqual(VM.VMState.FAULT, testengine.State);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentOverflow()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ByteArrayAssignment.cs");

            var result = testengine.ExecuteTestCaseStandard("testAssignmentOverflow").Pop();
            CollectionAssert.AreEqual(new byte[] { 0xff, 0x02, 0x03 }, result.GetSpan().ToArray());
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentWrongCasting()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ByteArrayAssignment.cs");

            testengine.ExecuteTestCaseStandard("testAssignmentWrongCasting");
            Assert.AreEqual(VM.VMState.FAULT, testengine.State);
        }

        [TestMethod]
        public void Test_ByteArrayAssignmentDynamic()
        {
            var testengine = new TestEngine();
            testengine.AddEntryScript("./TestClasses/Contract_ByteArrayAssignment.cs");

            var result = testengine.ExecuteTestCaseStandard("testAssignmentDynamic", 10);
            Assert.AreEqual(VM.VMState.HALT, testengine.State);
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x0a }, result.Pop().GetSpan().ToArray());
        }
    }
}
