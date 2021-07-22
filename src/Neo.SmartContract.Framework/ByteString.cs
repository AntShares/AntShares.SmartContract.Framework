using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Neo.SmartContract.Framework
{
    public abstract class ByteString : IEnumerable<byte>
    {
        public extern byte this[int index]
        {
            [OpCode(OpCode.PICKITEM)]
            get;
        }

        public extern int Length
        {
            [OpCode(OpCode.SIZE)]
            get;
        }

        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        [OpCode(OpCode.NOP)]
        public static extern implicit operator string(ByteString str);

        [OpCode(OpCode.NOP)]
        public static extern implicit operator ByteString(string str);

        [OpCode(OpCode.CONVERT, StackItemType.Buffer)]
        public static extern explicit operator byte[](ByteString str);

        [OpCode(OpCode.CONVERT, StackItemType.ByteString)]
        public static extern explicit operator ByteString(byte[] buffer);

        [OpCode(OpCode.DUP)]
        [OpCode(OpCode.ISNULL)]
        [OpCode(OpCode.JMPIFNOT, "0x04")]
        [OpCode(OpCode.DROP)]
        [OpCode(OpCode.PUSH0)]
        [OpCode(OpCode.CONVERT, StackItemType.Integer)]
        public static extern explicit operator BigInteger(ByteString text);

        [OpCode(OpCode.CONVERT, StackItemType.ByteString)]
        public static extern explicit operator ByteString(BigInteger integer);

        [OpCode(OpCode.CAT)]
        public static extern ByteString operator +(ByteString a);
    }
}
