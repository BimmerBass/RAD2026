using RadImplementationProject.Hashing;
using RadImplementationProject.Util;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Rendering;
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
            public ulong N { get; set; } // limit is 24

            [CommandOption("--l-samples")]
            [DefaultValue("1, 2, 4, 8, 12, 16, 20")]
            [TypeConverter(typeof(ListTypeConverter))]
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
            var rng = new Random(Extensions.SEED);
            var results = new List<(int, double, double, long)>();
            var ms = new MultiplyShift(settings.LSamples.First(), rng);
            var mmp = new MultiplyModPrime(settings.LSamples.First(), rng);

            AnsiConsole.Write(
                new FigletText("2nd Moment Exact Calculation")
                {
                    Justification = Justify.Center,
                    Color = Color.Green
                });
            AnsiConsole.MarkupLine($"n={settings.N}, l-value samples: {string.Join(", ", settings.LSamples)}");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]Multiply-shift[/]:     {ms.Format()}");
            AnsiConsole.MarkupLine($"[red]Multiply-mod-prime[/]: {mmp.Format()}");
            AnsiConsole.WriteLine();

            foreach (var lSample in settings.LSamples)
            {
                AnsiConsole.MarkupLine($"[green]Timing hash functions for l={lSample}[/]");

                // enumerate to ensure identical stream
                var stream = Stream.CreateStream((int)settings.N, lSample).ToList();
                ms.SetBitwidth(lSample);
                mmp.SetBitwidth(lSample);

                (TimeSpan Elapsed, long SecondMoment) msResult = (TimeSpan.FromMilliseconds(0), 0);
                (TimeSpan Elapsed, long SecondMoment) mmpResult = (TimeSpan.FromMilliseconds(0), 0);
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start("Timing Multiply-Shift...", ctx =>
                    {
                        msResult = stream.ComputeExactF2(ms);

                        ctx.Status("Timing Multiply-mod-Prime...");
                        mmpResult = stream.ComputeExactF2(mmp);
                    });

                AnsiConsole.MarkupLine($"[red]Multiply-shift[/]:\tL2={msResult.SecondMoment}, time={msResult.Elapsed.TotalMilliseconds} ms");
                AnsiConsole.MarkupLine($"[red]Multiply-mod-prime[/]:\tL2={mmpResult.SecondMoment}, time={mmpResult.Elapsed.TotalMilliseconds} ms");
                AnsiConsole.WriteLine();

                if (msResult.SecondMoment != mmpResult.SecondMoment)
                    throw new InvalidOperationException("second moments did not match");

                results.Add((lSample, msResult.Elapsed.TotalMilliseconds, mmpResult.Elapsed.TotalMilliseconds, mmpResult.SecondMoment));
            }
            var table = new Table()
                    .RoundedBorder()
                    .AddColumn("l-value")
                    .AddColumn("ms")
                    .AddColumn("mmp")
                    .AddColumn("L2");
            results.ForEach(r =>
            {
                table.AddRow(
                    r.Item1.ToString(),
                    r.Item2.ToString(),
                    r.Item3.ToString(),
                    r.Item4.ToString());
            });
            AnsiConsole.Write(table);
            return 0;
        }
    }
}
