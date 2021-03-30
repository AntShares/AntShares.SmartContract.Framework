using Neo.SmartContract.Framework.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Neo.SmartContract.Framework.UnitTests.TestClasses
{
    public class Contract_String : SmartContract.Framework.SmartContract
    {
        public static int TestStringAdd(string s1, string s2)
        {
            int a = 3;
            string c = s1 + s2;
            if (c == "hello")
            {
                a = 4;
            }
            else if (c == "world")
            {
                a = 5;
            }
            return a;
        }

        public static string TestStringAddInt(string s, int i)
        {
            return s + i;
        }
    }
}
