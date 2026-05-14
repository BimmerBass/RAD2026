using CommandLine;
using RadImplementationProject.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Tasks
{
    [Verb("13", aliases: ["quadraticsums"], HelpText = "Measure the time it takes to calculate ")]
    public class QuadraticSums
    {
        [Option('n', Default = (int)(1UL << 24))]
        public int NumStreamEntries { get; set; }
        [Option('u', Default = (int)(1UL << 16))]
        public int NumUniqueKeyValues { get; set; }

        [Option('l', Min =1, Default = new[] {1,2, 4, 8, 12, 16, 20})]
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
            if (args.LSamples.Any(l => (1UL << l) > (ulong)args.NumStreamEntries))
            {
                var first = args.LSamples.First(l => (1UL << l) > (ulong)args.NumStreamEntries);
                throw new ArgumentOutOfRangeException($"l={first} cannot have 2^l > n = ${args.NumStreamEntries}");
            }

            Console.WriteLine("Quadratic Sum Calculation Test");
            Console.WriteLine($"  - n: {args.NumStreamEntries}");
            Console.WriteLine($"  - l-values: [{string.Join(", ", args.LSamples)}]");
            Console.WriteLine("");

            var results = new List<Row>();
            var stream = Stream.CreateStream(args.NumStreamEntries, args.NumUniqueKeyValues).ToList();
            var multiplyShift = new MultiplyShift(args.LSamples.First(), rng);
            var multiplyModPrime = new MultiplyModPrime(args.LSamples.First(), rng);

            for (var i = 0; i < args.LSamples.Count(); i++)
            {
                var l = args.LSamples.ElementAt(i);
                multiplyShift.SetBitwidth(l);
                multiplyModPrime.SetBitwidth(l);

                Console.WriteLine($"[{i + 1}] Test for l={l}");
                Console.WriteLine($"[{i + 1}]   - Multiply-shift: {multiplyShift.Format()}");
                Console.WriteLine($"[{i + 1}]   - Multiply-mod-prime: {multiplyModPrime.Format()}");

                var multiplyShiftResults = CalculateSum(stream, multiplyShift);
                var multiplyModPrimeResults = CalculateSum(stream, multiplyModPrime);

                Console.WriteLine($"[{i + 1}]   Results for Multiply-shift:");
                Console.WriteLine($"[{i + 1}]     Sum: {multiplyShiftResults.Item1}");
                Console.WriteLine($"[{i + 1}]     Elapsed milliseconds: {multiplyShiftResults.Item2.TotalMilliseconds}");

                Console.WriteLine($"[{i + 1}]   Results for Multiply-mod-prime:");
                Console.WriteLine($"[{i + 1}]     Sum: {multiplyModPrimeResults.Item1}");
                Console.WriteLine($"[{i + 1}]     Elapsed milliseconds: {multiplyModPrimeResults.Item2.TotalMilliseconds}");
                Console.WriteLine("\n");

                results.Add(new Row
                {
                    L = l,
                    MultiplyShiftMs = multiplyShiftResults.Item2.TotalMilliseconds,
                    MultiplyModPrimeMs = multiplyModPrimeResults.Item2.TotalMilliseconds
                });
            }

            Console.WriteLine("CSV results: ");
            Console.WriteLine("");
            Console.WriteLine("n|l|multshift_ms|multmodprime_ms");
            foreach (var row in results)
            {
                var elements = new[] {args.NumStreamEntries.ToString(), row.L.ToString(), row.MultiplyShiftMs.ToString(), row.MultiplyModPrimeMs.ToString()};
                Console.WriteLine(string.Join("|", elements));
            }
            Console.WriteLine("");
        }

        public static (long, TimeSpan) CalculateSum(List<Tuple<ulong, int>> stream, IHashFunction function)
        {
            var sw = new Stopwatch();
            var table = new HashTable(function);
            var keys = new HashSet<ulong>();

            sw.Start();
            foreach (var entry in stream)
            {
                table.increment(entry.Item1, entry.Item2);
                keys.Add(entry.Item1);
            }

            var sum = keys
                .Select(k =>
                {
                    var t = (long)table.get(k);
                    return t * t;
                }).Sum();
            sw.Stop();

            return (sum, sw.Elapsed);
        }
    }
}
