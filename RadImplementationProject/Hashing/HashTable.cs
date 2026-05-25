using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Hashing
{
    public class HashTable
    {
        private class Entry
        {
            public ulong Key;
            public int Value;

            public static Entry Default => new Entry { Key = 0, Value = 0 };
        }
        readonly IReadOnlyList<List<Entry>> _buckets;
        readonly IHashFunction _function;

        public HashTable(IHashFunction function)
        {
            _function = function;
            _buckets = Enumerable
                .Range(0, (int)function.Range())
                .Select(_ => new List<Entry>())
                .ToList();
                
        }

        public int get(ulong x)
        {
            var key = _function.Hash(x);
            var bucket = _buckets[(int)key];

            var entry = bucket.FirstOrDefault(e => e.Key == x, Entry.Default);
            return entry.Value;
        }

        public void set(ulong x, int v)
        {
            var entry = FindOrCreate(x);
            entry.Value = v;
        }

        public void increment(ulong x, int d)
        {
            var entry = FindOrCreate(x);
            entry.Value += d;
        }

        private Entry FindOrCreate(ulong x)
        {
            var key = _function.Hash(x);
            var bucket = _buckets[(int)key];
            var entry =
                bucket.FirstOrDefault(e => e.Key == x) ??
                bucket.AddAndReturn(new Entry { Key = x, Value = 0 });
            return entry;
        }
    }
}
