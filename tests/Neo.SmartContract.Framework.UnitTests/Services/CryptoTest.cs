using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo.TestingEngine;
using Neo.Cryptography;
using Neo.Network.P2P;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using Neo.VM.Types;
using Neo.Wallets;
using System.Linq;
using System.Security.Cryptography;

namespace Neo.SmartContract.Framework.UnitTests.Services
{
    [TestClass]
    public class CryptoTest
    {
        private KeyPair _key = null;
        private TestEngine _engine;

        [TestInitialize]
        public void Init()
        {
            _engine = new TestEngine(TriggerType.Application, new Transaction()
            {
                Attributes = System.Array.Empty<TransactionAttribute>(),
                Script = System.Array.Empty<byte>(),
                Signers = new Signer[] { new Signer() { Account = UInt160.Zero } },
                Witnesses = System.Array.Empty<Witness>(),
                NetworkFee = 1,
                Nonce = 2,
                SystemFee = 3,
                ValidUntilBlock = 4,
                Version = 5
            }, new TestDataCache());
            _engine.AddEntryScript("./TestClasses/Contract_Crypto.cs");
            _key = GenerateKey(32);
        }

        public static KeyPair GenerateKey(int privateKeyLength)
        {
            byte[] privateKey = new byte[privateKeyLength];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(privateKey);
            }
            return new KeyPair(privateKey);
        }

        [TestMethod]
        public void Test_SHA256()
        {
            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("SHA256", "asd");
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Buffer));
            Assert.AreEqual("688787d8ff144c502c7f5cffaafe2cc588d86079f9de88304c26b0cb99ce91c6", item.GetSpan().ToArray().ToHexString());
        }

        [TestMethod]
        public void Test_RIPEMD160()
        {
            _engine.Reset();
            var str = System.Text.Encoding.Default.GetBytes("hello world");
            var result = _engine.ExecuteTestCaseStandard("RIPEMD160", str);
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Buffer));
            Assert.AreEqual("98c615784ccb5fe5936fbc0cbe9dfdb408d92f0f", item.GetSpan().ToArray().ToHexString());
        }

        [TestMethod]
        public void Test_VerifySignatureWithMessage()
        {
            byte[] signature = Crypto.Sign(_engine.ScriptContainer.GetSignData(ProtocolSettings.Default.Network),
                _key.PrivateKey, _key.PublicKey.EncodePoint(false).Skip(1).ToArray());

            // False

            _engine.Reset();
            var result = _engine.ExecuteTestCaseStandard("secp256r1VerifySignatureWithMessage",
                new ByteString(System.Array.Empty<byte>()),
                new ByteString(_key.PublicKey.EncodePoint(true)), new ByteString(signature));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            var item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsFalse(item.GetBoolean());

            // True

            _engine.Reset();
            result = _engine.ExecuteTestCaseStandard("secp256r1VerifySignatureWithMessage",
                new ByteString(_engine.ScriptContainer.GetSignData(ProtocolSettings.Default.Network)),
                new ByteString(_key.PublicKey.EncodePoint(true)), new ByteString(signature));
            Assert.AreEqual(VMState.HALT, _engine.State);
            Assert.AreEqual(1, result.Count);

            item = result.Pop();
            Assert.IsInstanceOfType(item, typeof(Boolean));
            Assert.IsTrue(item.GetBoolean());
        }
    }
}
