using Neo.IO.Json;
using Neo.SmartContract;
using Neo.SmartContract.Native;
using Neo.VM;

namespace Neo.TestingEngine
{
    class BuildNative : BuildScript
    {
        public readonly NativeContract NativeContract;
        public BuildNative(NativeContract nativeContract)
            : this(nativeContract, nativeContract.Nef, nativeContract.Manifest.ToJson())
        {
        }

        private BuildNative(NativeContract nativeContract, NefFile nef, JObject manifest) : base(nef, manifest)
        {
            this.NativeContract = nativeContract;
            this.DebugInfo = null;
        }
    }
}
