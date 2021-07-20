using Neo.Network.P2P.Payloads;
using Neo.Persistence;
using Neo.SmartContract;
using System.Collections.Generic;
using System.Linq;

namespace Neo.TestingEngine
{
    public class TestDataCache : DataCache
    {
        private readonly Dictionary<StorageKey, StorageItem> dict = new();

        public TestDataCache(Block persistingBlock = null)
        {
            this.DeployNativeContracts(persistingBlock);
        }

        protected override void AddInternal(StorageKey key, StorageItem value)
        {
            dict.Add(key, value);
        }

        protected override void DeleteInternal(StorageKey key)
        {
            dict.Remove(key);
        }

        protected override bool ContainsInternal(StorageKey key)
        {
            return dict.ContainsKey(key);
        }

        protected override StorageItem GetInternal(StorageKey key)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                return null;
            }

            return value;
        }

        protected override IEnumerable<(StorageKey Key, StorageItem Value)> SeekInternal(byte[] keyOrPrefix, SeekDirection direction)
        {
            return dict.Select(u => (u.Key, u.Value));
        }

        protected override StorageItem TryGetInternal(StorageKey key)
        {
            return dict.TryGetValue(key, out var value) ? value : null;
        }

        protected override void UpdateInternal(StorageKey key, StorageItem value)
        {
            dict[key] = value;
        }

        /// <summary>
        /// Include a new value to the storage for unit test
        /// </summary>
        public void AddForTest(StorageKey key, StorageItem value)
        {
            if (Contains(key))
            {
                Delete(key);
            }
            Add(key, value);
        }
    }
}
