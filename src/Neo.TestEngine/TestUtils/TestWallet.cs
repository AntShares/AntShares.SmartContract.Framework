using Neo.SmartContract;
using Neo.Wallets;
using Neo.Wallets.NEP6;
using System;
using System.Collections.Generic;

namespace Neo.TestingEngine
{
    public class TestWallet : NEP6Wallet
    {
        private Dictionary<UInt160, WalletAccount> accounts;
        public WalletAccount DefaultAccount { get; private set; }

        public TestWallet(UInt160 scriptHash = null) : base("", ProtocolSettings.Default, "TestWallet")
        {
            this.accounts = new Dictionary<UInt160, WalletAccount>();
            if (scriptHash == null)
            {
                // mock an account
                var mockedPrivateKey = "a8639cbc8dc867fab51487c1ff0565600999ac73136c95454309f4883854efba".HexToBytes();
                DefaultAccount = CreateAccount(mockedPrivateKey);
            }
            else
            {
                DefaultAccount = CreateAccount(scriptHash);
            }
        }

        public override bool ChangePassword(string oldPassword, string newPassword)
        {
            return false;
        }

        public override bool Contains(UInt160 scriptHash)
        {
            return this.accounts.ContainsKey(scriptHash);
        }

        public override WalletAccount CreateAccount(byte[] privateKey)
        {
            if (privateKey is null) throw new ArgumentNullException(nameof(privateKey));
            KeyPair key = new(privateKey);
            if (key.PublicKey.IsInfinity) throw new ArgumentException(null, nameof(privateKey));

            var account = new TestAccount(key);
            AddAccount(account);
            return account;
        }

        public override WalletAccount CreateAccount(Contract contract, KeyPair key = null)
        {
            throw new NotImplementedException();
        }

        public override WalletAccount CreateAccount(UInt160 scriptHash)
        {
            var account = new TestAccount(scriptHash);
            this.accounts[scriptHash] = account;
            return account;
        }

        private void AddAccount(TestAccount account)
        {
            if (!accounts.ContainsKey(account.ScriptHash))
            {
                accounts[account.ScriptHash] = account;
            }
        }

        public override bool DeleteAccount(UInt160 scriptHash)
        {
            if (!this.accounts.ContainsKey(scriptHash))
            {
                return false;
            }
            return accounts.Remove(scriptHash);
        }

        public override WalletAccount GetAccount(UInt160 scriptHash)
        {
            if (!this.accounts.ContainsKey(scriptHash))
            {
                return null;
            }
            return accounts[scriptHash];
        }

        public override IEnumerable<WalletAccount> GetAccounts()
        {
            return this.accounts.Values;
        }

        public override bool VerifyPassword(string password)
        {
            return true;
        }

        internal void AddSignerAccount(UInt160 scriptHash)
        {
            // mock for calculating network fee
            this.accounts[scriptHash] = DefaultAccount;
        }
    }
}
