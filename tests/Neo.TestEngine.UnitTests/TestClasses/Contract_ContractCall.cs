using Neo.SmartContract.Framework;

namespace Neo.TestEngine.UnitTests.TestClasses
{
    [Contract("0x4281dd379f0831b4131f9bc3433299e4fda02e68")]
    public class Contract1
    {
        public static extern byte[] testArgs1(byte a);
        public static extern void testVoid();
    }

    public class Contract_ContractCall : SmartContract.Framework.SmartContract
    {
        public static byte[] testContractCall()
        {
            return Contract1.testArgs1((byte)4);
        }

        public static void testContractCallVoid()
        {
            Contract1.testVoid();
        }
    }
}
