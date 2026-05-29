using RadImplementationProject.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject
{
    public static class Stream
    {
        // Page four in project description
        // NOTE: This function is unchanged relative to the requirements document (Implementeringsprojekt.pdf)
        public static IEnumerable<Tuple<ulong, int>> CreateStream(int n, int l)
        {
            // We generate a random uint64 number .
            Random rnd = new System.Random();
            ulong a = 0UL;
            Byte[] b = new Byte[8];
            rnd.NextBytes(b);
            for (int i = 0; i < 8; ++i)
            {
                a = (a << 8) + (ulong)b[i];
            }

            // We demand that our random number has 30 zeros on the least
            // significant bits and then a one.
            a = (a | ((1UL << 31) - 1UL)) ^ ((1UL << 30) - 1UL);
            ulong x = 0UL;
            for (int i = 0; i < n / 3; ++i)
            {
                x = x + a;
                yield return Tuple.Create(x & (((1UL << l) - 1UL) << 30), 1);
            }

            for (int i = 0; i < (n + 1) / 3; ++i)
            {
                x = x + a;
                yield return Tuple.Create(x & (((1UL << l) - 1UL) << 30), -1);
            }
            for (int i = 0; i < (n + 2) / 3; ++i)
            {
                x = x + a;
                yield return Tuple.Create(x & (((1UL << l) - 1UL) << 30), 1);
            }
        }

        public static (TimeSpan Elapsed, long SecondMoment) ComputeExactF2(this List<Tuple<ulong, int>> stream, IHashFunction hash)
        {
            var sw = Stopwatch.StartNew();
            var rng = new Random(Extensions.SEED);
            var table = new HashTable(hash);
            var keys = new HashSet<ulong>();

            foreach (var entry in stream)
            {
                table.increment(entry.Item1, entry.Item2);
                keys.Add(entry.Item1);
            }

            var secondMoment = keys
                .Select(k => (long)table.get(k))
                .Select(freq => freq * freq)
                .Sum();
            sw.Stop();
            return (sw.Elapsed, secondMoment);
        }
    }
}
