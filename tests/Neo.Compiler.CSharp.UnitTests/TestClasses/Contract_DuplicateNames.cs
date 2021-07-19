using System.ComponentModel;
using Neo.SmartContract.Framework;

namespace Neo.Compiler.CSharp.UnitTests.TestClasses
{
    public class Contract_DuplicateNames : SmartContract.Framework.SmartContract
    {
        public delegate void OnSomethingDelegate(ByteString data);
        public delegate void OnSomethingElseDelegate(ByteString data);

        [DisplayName("OnSomething")]
        public static event OnSomethingDelegate Something;

        [DisplayName("OnSomething")]
        public static event OnSomethingElseDelegate SomethingElse;
    }
}
