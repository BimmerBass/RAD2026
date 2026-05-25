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
    [Verb("11c", aliases: ["hashtest"], HelpText = "Test multiply-mod-prime vs multiply-shift hashing.")]
    public class HashFunctionsTest
    {
        [Option('n',Required = true)]
        public int N { get; set; }
        [Option('l', Default = 20)]
        public int L { get; set; }

        public static void Run(HashFunctionsTest args)
        {
            var rng = new Random(Extensions.SEED);
            if (args.L < 1 || args.L >= 64)
                throw new ArgumentOutOfRangeException(nameof(args.L));
            if (args.N <= 0)
                throw new ArgumentOutOfRangeException(nameof(args.N));
            if ((1UL << args.L) > (ulong)args.N)
                throw new ArgumentOutOfRangeException($"l={args.L} cannot have 2^l > n = ${args.N}");

            var multiplyShift = new MultiplyShift(args.L, rng);
            var multiplyModPrime = new MultiplyModPrime(args.L, rng);

            Console.WriteLine("Hash function test");
            Console.WriteLine($"  n = {args.N}");
            Console.WriteLine($"  l = {args.L}");
            Console.WriteLine($"  2^l = {(1Ul << args.L)}");
            Console.WriteLine();

            Console.WriteLine("Testing hash functions:");
            Console.WriteLine($"  Multiply-shift:     {multiplyShift.Format()}");
            Console.WriteLine($"  Multiply-mod-prime: {multiplyModPrime.Format()}");
            Console.WriteLine();

            var stream = Stream.CreateStream(args.N, args.L);
            var multiplyShiftResults = MeasureFunction(multiplyShift, stream);
            var multiplyModPrimeResults = MeasureFunction(multiplyModPrime, stream);

            Console.WriteLine("Results for Multiply-shift:");
            Console.WriteLine($"  Sum: {multiplyShiftResults.Item1}");
            Console.WriteLine($"  Elapsed milliseconds: {multiplyShiftResults.Item2.TotalMilliseconds}");

            Console.WriteLine("Results for Multiply-mod-prime:");
            Console.WriteLine($"  Sum: {multiplyModPrimeResults.Item1}");
            Console.WriteLine($"  Elapsed milliseconds: {multiplyModPrimeResults.Item2.TotalMilliseconds}");
        }

        static (ulong, TimeSpan) MeasureFunction(IHashFunction function, IEnumerable<Tuple<ulong, int>> stream)
        {
            var sw = new Stopwatch();
            var sum = 0UL;

            sw.Start();
            foreach (var elem in stream)
                sum += function.Hash(elem.Item1);
            sw.Stop();
            return (sum, sw.Elapsed);
        }
    }
}
