namespace Neo.Compiler.CSharp.UnitTests.TestClasses
{
    public class Contract_ByteArrayAssignment : SmartContract.Framework.SmartContract
    {
        public static byte[] TestAssignment()
        {
            var a = new byte[] { 0x00, 0x02, 0x03 };
            a[0] = 0x01;
            a[2] = 0x04;
            return a;
        }

        public static byte[] testAssignmentOutOfBounds()
        {
            var a = new byte[] { 0x00, 0x02, 0x03 };
            a[0] = 0x01;
            a[3] = 0x04;
            return a;
        }

        public static byte[] testAssignmentOverflow()
        {
            var a = new byte[] { 0x00, 0x02, 0x03 };
            a[0] = (byte)int.MaxValue;
            return a;
        }

        public static byte[] testAssignmentWrongCasting()
        {
            object obj = "test";
            var a = new byte[] { 0x00, 0x02, 0x03 };
            a[0] = (byte)obj;
            return a;
        }
    }
}