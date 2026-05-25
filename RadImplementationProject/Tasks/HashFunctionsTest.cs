using CommandLine;
using RadImplementationProject.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadImplementationProject.Tasks
{
    [Verb("11c", aliases: [ "hashtest" ], HelpText = "Test multiply-mod-prime vs multiply-shift hashing.")]
    public class HashFunctionsTest
    {
        [Option('n', Required = true)]
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
                throw new ArgumentOutOfRangeException($"l={args.L} cannot have 2^l > n = {args.N}");

            var multiplyShift = new MultiplyShift(args.L, rng);
            var multiplyModPrime = new MultiplyModPrime(args.L, rng);

            Console.WriteLine("Hash function test");
            Console.WriteLine($"n={args.N} l={args.L} buckets={(1UL << args.L)}");
            Console.WriteLine($"Multiply-shift:     {multiplyShift.Format()}");
            Console.WriteLine($"Multiply-mod-prime: {multiplyModPrime.Format()}");
            Console.WriteLine();

            var stream = Stream.CreateStream(args.N, args.L);

            var shiftResult = MeasureFunction(multiplyShift, stream);
            var modResult = MeasureFunction(multiplyModPrime, stream);

            Console.WriteLine();
            Console.WriteLine("CSV results");
            Console.WriteLine("n|l|function|sum|ms");
            Console.WriteLine($"{args.N}|{args.L}|MultiplyShift|{shiftResult.Sum}|{shiftResult.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"{args.N}|{args.L}|MultiplyModPrime|{modResult.Sum}|{modResult.Elapsed.TotalMilliseconds}");
        }

        private static (ulong Sum, TimeSpan Elapsed) MeasureFunction(IHashFunction function, IEnumerable<Tuple<ulong, int>> stream)
        {
            var sw = Stopwatch.StartNew();
            ulong sum = 0UL;

            foreach (var item in stream)
            {
                sum += function.Hash(item.Item1);
            }

            sw.Stop();
            return (sum, sw.Elapsed);
        }
    }
}
