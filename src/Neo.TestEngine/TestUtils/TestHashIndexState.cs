using Neo.IO;
using Neo.SmartContract;
using Neo.VM;
using Neo.VM.Types;

namespace Neo.TestingEngine
{
    public class TestHashIndexState : IInteroperable
    {
        public UInt256 Hash;
        public uint Index;

        internal void FromStackItem(StackItem stackItem)
        {
            Struct @struct = (Struct)stackItem;
            Hash = new UInt256(@struct[0].GetSpan());
            Index = (uint)@struct[1].GetInteger();
        }

        void IInteroperable.FromStackItem(StackItem stackItem)
        {
            this.FromStackItem(stackItem);
        }

        internal StackItem ToStackItem(ReferenceCounter referenceCounter)
        {
            return new Struct(referenceCounter) { Hash.ToArray(), Index };
        }

        StackItem IInteroperable.ToStackItem(ReferenceCounter referenceCounter)
        {
            return this.ToStackItem(referenceCounter);
        }
    }
}
