using CommandLine;
using RadImplementationProject.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadImplementationProject.Tasks
{
    [Verb("13", aliases: ["quadraticsums"], HelpText = "Measure quadratic sum time for different bitwidths.")]
    public class QuadraticSums
    {
        [Option('n', Default = (int)(1UL << 24))]
        public int NumStreamEntries { get; set; }

        [Option('u', Default = (int)(1UL << 16))]
        public int NumUniqueKeyValues { get; set; }

        [Option('l', Min = 1, Default = new[] { 1, 2, 4, 8, 12, 16, 20 })]
        public required IEnumerable<int> LSamples { get; set; }

        class Row
        {
            public int L { get; set; }
            public double MultiplyShiftMs { get; set; }
            public double MultiplyModPrimeMs { get; set; }
        }

        public static void Run(QuadraticSums args)
        {
            var rng = new Random(Extensions.SEED);

            if (args.LSamples.Any(l => l < 1 || l >= 64))
                throw new ArgumentOutOfRangeException(nameof(args.LSamples));
            if (args.NumStreamEntries <= 0)
                throw new ArgumentOutOfRangeException(nameof(args.NumStreamEntries));

            var firstBad = args.LSamples.FirstOrDefault(l => (1UL << l) > (ulong)args.NumStreamEntries);
            if (firstBad != 0)
                throw new ArgumentOutOfRangeException($"l={firstBad} cannot have 2^l > n = {args.NumStreamEntries}");

            Console.WriteLine("Quadratic Sum Calculation Test");
            Console.WriteLine($"n={args.NumStreamEntries}, unique_keys={args.NumUniqueKeyValues}");
            Console.WriteLine($"l-values: [{string.Join(", ", args.LSamples)}]");

            var results = new List<Row>();
            var stream = Stream.CreateStream(args.NumStreamEntries, args.NumUniqueKeyValues).ToList();
            var multiplyShift = new MultiplyShift(args.LSamples.First(), rng);
            var multiplyModPrime = new MultiplyModPrime(args.LSamples.First(), rng);

            Console.WriteLine($"Multiply-shift:     {multiplyShift.Format()}");
            Console.WriteLine($"Multiply-mod-prime: {multiplyModPrime.Format()}");
            Console.WriteLine();

            int i = 0;
            foreach (var l in args.LSamples)
            {
                i++;
                multiplyShift.SetBitwidth(l);
                multiplyModPrime.SetBitwidth(l);

                Console.WriteLine($"[{i}] Testing l={l}");
                var shiftResult = CalculateSum(stream, multiplyShift);
                var modResult = CalculateSum(stream, multiplyModPrime);
                Console.WriteLine($"    Multiply-shift: sum={shiftResult.Sum} ms={shiftResult.Elapsed.TotalMilliseconds}");
                Console.WriteLine($"    Multiply-mod-prime: sum={modResult.Sum} ms={modResult.Elapsed.TotalMilliseconds}");
                Console.WriteLine();

                results.Add(new Row
                {
                    L = l,
                    MultiplyShiftMs = shiftResult.Elapsed.TotalMilliseconds,
                    MultiplyModPrimeMs = modResult.Elapsed.TotalMilliseconds
                });
            }

            Console.WriteLine("CSV results:");
            Console.WriteLine("n|l|multshift_ms|multmodprime_ms");
            foreach (var row in results)
            {
                Console.WriteLine($"{args.NumStreamEntries}|{row.L}|{row.MultiplyShiftMs}|{row.MultiplyModPrimeMs}");
            }
            Console.WriteLine();
        }

        public static (long Sum, TimeSpan Elapsed) CalculateSum(List<Tuple<ulong, int>> stream, IHashFunction function)
        {
            var sw = Stopwatch.StartNew();
            var table = new HashTable(function);
            var keys = new HashSet<ulong>();

            foreach (var entry in stream)
            {
                table.increment(entry.Item1, entry.Item2);
                keys.Add(entry.Item1);
            }

            long sum = keys.Select(k => (long)table.get(k)).Select(t => t * t).Sum();

            sw.Stop();
            return (sum, sw.Elapsed);
        }
    }
}
