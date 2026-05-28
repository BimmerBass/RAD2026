using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Tasks
{
    public class QuadraticSumsCommand : Command<QuadraticSumsCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--stream-size")]
            [DefaultValue(1UL << 24)]
            public ulong N { get; set; }
            
            [CommandOption("--key-universe")]
            [DefaultValue(1UL << 16)]
            public ulong U { get; set; }

            [CommandOption("--l-samples")]
            [DefaultValue(new[] { 1, 2, 4, 8, 12, 16, 20 })]
            public required IEnumerable<int> LSamples { get; set; }
        }

        protected override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (settings.LSamples.Any(l => l < 1 || l >= 64))
                return ValidationResult.Error("All l-value samples must be in the interval [1;64)");
            if (settings.N < 1)
                return ValidationResult.Error("stream-size must be a positive number");

            var firstBad = settings.LSamples.FirstOrDefault(l => (1UL << l) > settings.N);
            if (firstBad != 0)
                return ValidationResult.Error($"l={firstBad} cannot have 2^l > n = {settings.N}");

            return base.Validate(context, settings);
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            /*
             
             public static void Run(QuadraticSums args)
        {
            var rng = new Random(Extensions.SEED);

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
                var shiftResult = Stream.ComputeExactF2(stream, multiplyShift);
                var modResult = Stream.ComputeExactF2(stream, multiplyModPrime);
                Console.WriteLine($"    Multiply-shift: sum={shiftResult.SecondMoment} ms={shiftResult.Elapsed.TotalMilliseconds}");
                Console.WriteLine($"    Multiply-mod-prime: sum={modResult.SecondMoment} ms={modResult.Elapsed.TotalMilliseconds}");
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
             
             */
            throw new NotImplementedException();
        }
    }
}
