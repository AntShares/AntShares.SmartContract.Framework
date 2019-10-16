namespace Neo.SmartContract.Framework.Services.Neo
{
    public class Contract
    {
        /// <summary>
        /// Script
        /// </summary>
        public readonly byte[] Script;

        /// <summary>
        /// Has storage
        /// </summary>
        public readonly bool HasStorage;

        /// <summary>
        /// Is payable
        /// </summary>
        public readonly bool IsPayable;

        [Syscall("Neo.Contract.Call")]
        public static extern Contract Call(byte[] scriptHash, string method, object[] arguments);

        [Syscall("Neo.Contract.Create")]
        public static extern Contract Create(byte[] script, string manifest);

        [Syscall("Neo.Contract.Update")]
        public static extern Contract Update(byte[] script, string manifest);

        [Syscall("Neo.Contract.Migrate")]
        public static extern Contract Migrate(byte[] script, ContractPropertyState contract_property_state);

        [Syscall("System.Contract.Destroy")]
        public static extern void Destroy();
    }
}
