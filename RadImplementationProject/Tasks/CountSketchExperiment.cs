using RadImplementationProject.Hashing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RadImplementationProject.Tasks
{
    /*[Verb("21", aliases: ["count-sketch"], HelpText = "Benchmark Count-Sketch estimation accuracy.")]
    public class CountSketchExperiment
    {
        [Option('n', Default = (int)(1UL << 20))]
        public int NumStreamEntries { get; set; }

        [Option('u', Default = (int)(1UL << 16))]
        public int NumUniqueKeyValues { get; set; }

        [Option('t', Min = 1, Default = new[] { 4, 8, 12, 16, 20 })]
        public required IEnumerable<int> TSamples { get; set; }

        [Option("trials", Default = 100)]
        public int NumTrials { get; set; }

        class Row
        {
            public int T { get; set; }
            public long Estimate { get; set; }
            public long Actual { get; set; }
            public double ErrorPercent { get; set; }
        }

        public static void Run(CountSketchExperiment args)
        {
            var rng = new Random(Extensions.SEED);

            if (args.TSamples.Any(t => t < 1 || t > 64))
                throw new ArgumentOutOfRangeException(nameof(args.TSamples));
            if (args.NumStreamEntries <= 0)
                throw new ArgumentOutOfRangeException(nameof(args.NumStreamEntries));
            if (args.NumUniqueKeyValues <= 0 || args.NumUniqueKeyValues >= args.NumStreamEntries)
                throw new ArgumentOutOfRangeException(nameof(args.NumUniqueKeyValues), 
                    $"Must be > 0 and < NumStreamEntries ({args.NumStreamEntries})");
            if (args.NumTrials <= 0)
                throw new ArgumentOutOfRangeException(nameof(args.NumTrials));

            Console.WriteLine("Count-Sketch Experiment");
            Console.WriteLine($"n={args.NumStreamEntries}, unique_keys={args.NumUniqueKeyValues}, trials={args.NumTrials}");
            Console.WriteLine($"t-values: [{string.Join(", ", args.TSamples)}]");
            Console.WriteLine();

            var stream = Stream.CreateStream(args.NumStreamEntries, args.NumUniqueKeyValues).ToList();
            var exactF2 = Stream.ComputeExactF2(stream, new MultiplyShift(20, rng)).SecondMoment;

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
