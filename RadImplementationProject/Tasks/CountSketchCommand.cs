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

            [CommandOption("--m-bit-widths")]
            [DefaultValue("23")]
            [TypeConverter(typeof(ListTypeConverter))]
            public required IEnumerable<int> MBitWidths { get; set; }

            [CommandOption("--csv-path")]
            public required string CsvPath { get; set;  }
        }

        protected override ValidationResult Validate(CommandContext context, Settings settings)
        {
            if (settings.L < 1 || settings.L >= 64)
                return ValidationResult.Error("bit-width must be in the interval [1;64).");
            if (settings.N < 1)
                return ValidationResult.Error("stream-size must be a positive number");
            if (settings.MBitWidths == null || !settings.MBitWidths.Any())
                return ValidationResult.Error("at least one m-bit-width must be provided.");
            if (settings.MBitWidths.Any(bitWidth => bitWidth < 1 || bitWidth >= 64))
                return ValidationResult.Error("all m-bit-width values must be in the interval [1;64).");

            var numUniqueKeys = 1UL << settings.L;
            if (numUniqueKeys > settings.N)
                return ValidationResult.Error("stream-size cannot be smaller than the key universe (2^bit-width)");

            var csvPath = Path.GetFullPath(settings.CsvPath);
            var dir = Path.GetDirectoryName(csvPath);
            if (csvPath == null || !Directory.Exists(dir))
                return ValidationResult.Error("invalid path");

            return base.Validate(context, settings);
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var rng = new Random(Extensions.SEED);
            var stream = Stream
                .CreateStream((int)settings.N, settings.L)
                .ToList();
            var mBitWidths = settings.MBitWidths
                .Distinct()
                .Order()
                .ToList();
            var rows = new List<CountSketchEstimate>();
            var summaries = new List<(int MBitWidth, double Mse, double CountSketchMilliseconds, double ExactF2Milliseconds)>();

            AnsiConsole.Write(
                new FigletText("CountSketch Experiment")
                {
                    Justification = Justify.Center,
                    Color = Color.Green
                });
            AnsiConsole.MarkupLine($"n={settings.N}, bit-width={settings.L}, m-bit-widths={string.Join(", ", mBitWidths)}");
            AnsiConsole.WriteLine();

            (TimeSpan Elapsed, long SecondMoment) exactResult = (TimeSpan.Zero, 0L);
            AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .Start("Computing Exact Second Moment", ctx =>
                {
                    exactResult = stream.ComputeExactF2(new MultiplyShift(settings.L, rng));
                });
            AnsiConsole.MarkupLine($"[bold green]Exact F-2:[/] {exactResult.SecondMoment}");
            AnsiConsole.MarkupLine($"[bold green]Exact F-2 time:[/] {exactResult.Elapsed.TotalMilliseconds} ms");

            var progressLock = new object();
            AnsiConsole.Progress()
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new ElapsedTimeColumn(),
                    new RemainingTimeColumn())
                .StartAsync(async ctx =>
                {
                    var tasks = mBitWidths
                        .Select(mBitWidth => (
                            MBitWidth: mBitWidth,
                            Progress: ctx.AddTask($"m=2^{mBitWidth}", maxValue: 100)))
                        .Select(item => Task.Run(() => RunCountSketchExperiment(stream, exactResult, item.MBitWidth, item.Progress, progressLock), cancellationToken))
                        .ToList();

                    var results = await Task.WhenAll(tasks);
                    rows.AddRange(results.SelectMany(result => result.Rows));
                    summaries.AddRange(results.Select(result => (
                        result.MBitWidth,
                        result.Mse,
                        result.CountSketchMilliseconds,
                        result.ExactF2Milliseconds)));
                })
                .GetAwaiter()
                .GetResult();

            var table = new Table()
                    .RoundedBorder()
                    .AddColumn("m-bit-width")
                    .AddColumn("m")
                    .AddColumn("MSE")
                    .AddColumn("CountSketch time (ms)")
                    .AddColumn("Exact F2 time (ms)");
            summaries
                .OrderBy(summary => summary.MBitWidth)
                .ToList()
                .ForEach(summary => table.AddRow(
                    summary.MBitWidth.ToString(),
                    $"2^{summary.MBitWidth}",
                    summary.Mse.ToString(CultureInfo.InvariantCulture),
                    summary.CountSketchMilliseconds.ToString(CultureInfo.InvariantCulture),
                    summary.ExactF2Milliseconds.ToString(CultureInfo.InvariantCulture)));
            AnsiConsole.Write(table);

            using (var writer = new StreamWriter(settings.CsvPath, false))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows.OrderBy(row => row.CounterBitWidth).ThenBy(row => row.Index));
            }

            return 0;
        }

        private static CountSketchExperimentResult RunCountSketchExperiment(
            IReadOnlyList<Tuple<ulong, int>> stream,
            (TimeSpan Elapsed, long SecondMoment) exactResult,
            int mBitWidth,
            ProgressTask progress,
            object progressLock)
        {
            var rng = new Random(Extensions.SEED + mBitWidth);
            var estimates = new List<long>();

            var sw = Stopwatch.StartNew();
            for (var index = 0; index < 100; index++)
            {
                var cs = new CountSketch(mBitWidth, rng);
                foreach (var entry in stream)
                    cs.Update(entry.Item1, entry.Item2);

                estimates.Add(cs.EstimateF2());
                lock (progressLock)
                    progress.Increment(1);
            }
            sw.Stop();

            var sortedEstimates = estimates.Order().ToList();
            var mse = estimates
                .Select(estimate => (double)estimate - exactResult.SecondMoment)
                .Select(error => error * error)
                .Average();
            var rows = estimates
                .Select((estimate, index) => new CountSketchEstimate
                {
                    Index = index + 1,
                    Estimate = estimate,
                    SortedEstimate = sortedEstimates[index],
                    ExactF2 = exactResult.SecondMoment,
                    MeanSquareError = mse,
                    CountSketchMilliseconds = sw.Elapsed.TotalMilliseconds,
                    ExactF2Milliseconds = exactResult.Elapsed.TotalMilliseconds,
                    CounterBitWidth = mBitWidth
                })
                .ToList();

            return new CountSketchExperimentResult(
                mBitWidth,
                rows,
                mse,
                sw.Elapsed.TotalMilliseconds,
                exactResult.Elapsed.TotalMilliseconds);
        }

        private sealed record CountSketchExperimentResult(
            int MBitWidth,
            IReadOnlyList<CountSketchEstimate> Rows,
            double Mse,
            double CountSketchMilliseconds,
            double ExactF2Milliseconds);
    }
}
