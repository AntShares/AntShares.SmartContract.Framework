using System;
using System.ComponentModel;

namespace Neo.TestEngine.UnitTests.TestClasses
{
    public class Contract2 : SmartContract.Framework.SmartContract
    {
        [DisplayName("event")]
        public static event Action<object> notify;

        public static byte UnitTest_002(object arg1, object arg2)
        {
            notify(arg1);
            notify(arg2);
            var nb = new byte[] { 1, 2, 3, 4 };
            return nb[2];
        }
    }
}
