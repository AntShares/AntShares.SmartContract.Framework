using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace Neo.SmartContract.Framework.UnitTests.TestClasses
{
    public class Contract_Runtime : SmartContract
    {
        public static ulong GetTime()
        {
            return Runtime.Time;
        }

        public static object GetBlock(uint index)
        {
            return Ledger.GetBlock(index);
        }
    }
}
