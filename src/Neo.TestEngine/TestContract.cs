using Neo.SmartContract;

namespace Neo.TestingEngine
{
    public class TestContract
    {
        internal string nefPath;
        internal object buildScript = null;

        public TestContract(string path)
        {
            nefPath = path;
        }
    }
}
