using Neo.SmartContract;

namespace Neo.TestingEngine
{
    internal class TestNefFile : NefFile
    {
        public TestNefFile(byte[] script) : base()
        {
            Script = script;
            Compiler = "mock";
            Tokens = new MethodToken[0];
        }
    }
}
