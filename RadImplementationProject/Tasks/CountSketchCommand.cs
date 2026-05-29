using CsvHelper;
using RadImplementationProject.Hashing;
using RadImplementationProject.Util;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
            [DefaultValue(23)]
            public int L { get; set; }

            [CommandOption("--csv-path")]
            public required string CsvPath { get; set;  }
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

            var csvPath = Path.GetFullPath(settings.CsvPath);
            var dir = Path.GetDirectoryName(csvPath);
            if (csvPath == null || !Directory.Exists(dir))
                return ValidationResult.Error("invalid path");

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

            var table = new Table()
                    .RoundedBorder()
                    .AddColumn("index")
                    .AddColumn("estimate");
            estimates
                .Select((x, i) => (x, i)).ToList()
                .ForEach(t => table.AddRow(t.i.ToString(), t.x.ToString()));
            AnsiConsole.Write(table);

            var estimatesSorted = estimates.Order().ToList();
            var mseSum = (double)estimates
                .Select(e => e - exactF2)
                .Select(r => r * r)
                .Sum();
            var mse = mseSum / (double)estimates.Count;

            var rows = estimates.Select((e, i) => new CountSketchEstimate
            {
                Index = i + 1,
                Estimate = e,
                SortedEstimate = estimatesSorted[i],
                ExactF2 = exactF2,
                MeanSquareError = mse
            });

            using (var writer = new StreamWriter(settings.CsvPath, false))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }

            return 0;
        }
    }
}
