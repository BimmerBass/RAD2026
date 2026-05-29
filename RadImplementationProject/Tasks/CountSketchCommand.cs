using RadImplementationProject.Hashing;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace RadImplementationProject.Tasks
{
    public class CountSketchCommand : Command<CountSketchCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("--stream-size")]
            [DefaultValue(1UL << 24)]
            public ulong N { get; set; }

            [CommandOption("--bit-width")]
            [DefaultValue(12)]
            public int L { get; set; }
        }

        protected override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (settings.L < 1 || settings.L >= 64)
                return ValidationResult.Error("bit-width must be in the interval [1;64).");
            if (settings.N < 1)
                return ValidationResult.Error("stream-size must be a positive number");
            var numUniqueKeys = 1UL << settings.L;
            if (numUniqueKeys > settings.N)
                return ValidationResult.Error("stream-size cannot be smaller than the key universe (2^L)");
            return base.Validate(context, settings);
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var rng = new Random();
            
            var stream = Stream
                .CreateStream((int)settings.N, settings.L)
                .ToList();
            AnsiConsole.Write(
                new FigletText("CountSketch Experiment")
                {
                    Justification = Justify.Center,
                    Color = Color.Green
                });
            AnsiConsole.MarkupLine($"n={settings.N}, l={settings.L}");
            AnsiConsole.WriteLine();

            var exactF2 = 0L;
            AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots)
                    .Start("Computing Exact Second Moment", ctx =>
                    {
                        exactF2 = stream.ComputeExactF2(new MultiplyShift(settings.L, rng)).SecondMoment;
                    });
            AnsiConsole.MarkupLine($"[bold green]Exact F-2:[/] {exactF2}");

            var estimates = new List<long>();
            AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn())
                .Start(ctx =>
                {
                    var task = ctx.AddTask("Computing F2-estimates", maxValue: 100);
  
                    for (var _ = 0; _ < 100; _++)
                    {
                        var cs = new CountSketch(settings.L, rng);
                        foreach (var entry in stream)
                            cs.Update(entry.Item1, entry.Item2);

                        estimates.Add(cs.EstimateF2());
                        task.Increment(1);
                    }
                });
            estimates.Sort();

            var table = new Table()
                    .RoundedBorder()
                    .AddColumn("index")
                    .AddColumn("estimate");
            estimates
                .Select((x, i) => (x, i)).ToList()
                .ForEach(t => table.AddRow(t.i.ToString(), t.x.ToString()));
            AnsiConsole.Write(table);
            return 0;
        }
    }
    /*var exactF2 = Stream.ComputeExactF2(stream, new MultiplyShift(20, rng)).SecondMoment;

            var results = new List<Row>();

            int sampleIndex = 0;
            foreach (var t in args.TSamples)
            {
                sampleIndex++;
                Console.WriteLine($"[{sampleIndex}] Testing t={t} (m={1UL << t})");
                
                var trialEstimates = new List<long>();

                for (int trial = 0; trial < args.NumTrials; trial++)
                {
                    var trialRng = new Random(Extensions.SEED + trial);
                    var sketch = new CountSketch(t, trialRng);

                    foreach (var (x, d) in stream)
                    {
                        sketch.Update(x, d);
                    }

                    var estimate = sketch.Estimate();
                    trialEstimates.Add(estimate);
                }

                trialEstimates.Sort();
                var medianEstimate = trialEstimates[args.NumTrials / 2];
                var errorPercent = exactF2 == 0 ? 0 : 100.0 * Math.Abs(medianEstimate - exactF2) / exactF2;

                Console.WriteLine($"    Median estimate: {medianEstimate}, Exact F2: {exactF2}, Error: {errorPercent:F2}%");
                Console.WriteLine();

                results.Add(new Row
                {
                    T = t,
                    Estimate = medianEstimate,
                    Actual = exactF2,
                    ErrorPercent = errorPercent
                });
            }

            Console.WriteLine("CSV results:");
            Console.WriteLine("n|t|estimate|actual|error_percent");
            foreach (var row in results)
            {
                Console.WriteLine($"{args.NumStreamEntries}|{row.T}|{row.Estimate}|{row.Actual}|{row.ErrorPercent:F2}");
            }
            Console.WriteLine();
        }
    }*/
}
