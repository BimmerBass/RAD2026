using RadImplementationProject.Hashing;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Tasks
{
    public class HashFunctionsCommand : Command<HashFunctionsCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--stream-size")]
            [DefaultValue(1UL << 24)]
            public ulong N { get; set; }
            [CommandOption("--bit-width")]
            [DefaultValue(20)]
            public int L { get; set; }
        }

        protected override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (settings.L < 1 || settings.L >= 64)
                return ValidationResult.Error("bit-width must be in the interval [1;64).");
            if (settings.N < 1)
                return ValidationResult.Error("stream-size must be a positive integer.");

            var numUniqueKeys = 1UL << settings.L;
            if (numUniqueKeys > settings.N)
                return ValidationResult.Error("stream-size cannot be smaller than the key universe (2^L)");

            return base.Validate(context, settings);
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var rng = new Random(Extensions.SEED);
            var multiplyShift = new MultiplyShift(settings.L, rng);
            var multiplyModPrime = new MultiplyModPrime(settings.L, rng);

            //AnsiConsole.MarkupLine("[bold green]Hash Function Test[/]");
            AnsiConsole.Write(
                new FigletText("Hash Test")
                {
                    Justification = Justify.Center,
                    Color = Color.Green
                });

            AnsiConsole.MarkupLine($"n={settings.N} l={settings.L} buckets={(1UL << settings.L)}");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Multiply-shift[/]:     {multiplyShift.Format()}");
            AnsiConsole.MarkupLine($"[red]Multiply-mod-prime[/]: {multiplyModPrime.Format()}");
            AnsiConsole.WriteLine();

            var stream = Stream.CreateStream((int)settings.N, settings.L).ToList();
            var shiftResult = MeasureFunction(multiplyShift, stream);
            var modResult = MeasureFunction(multiplyModPrime, stream);

            AnsiConsole.Write(
                new Table()
                    .RoundedBorder()
                    .AddColumn("Function")
                    .AddColumn("Sum")
                    .AddColumn("Calculation time (ms)")
                    .AddRow("Multiply-Shift", shiftResult.Sum.ToString(), shiftResult.Elapsed.TotalMilliseconds.ToString())
                    .AddRow("Multiply-mod-Prime", modResult.Sum.ToString(), modResult.Elapsed.TotalMilliseconds.ToString()));

            return 0;
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
