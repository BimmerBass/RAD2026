using System;
using System.Linq;

namespace RadImplementationProject.Hashing
{
    public class CountSketch
    {
        private readonly CountSketchHashFunctions hashFunctions;
        private readonly long[] counters;

        public CountSketch(int bitwidth, Random rng)
        {
            if (bitwidth < 1 || bitwidth > 64)
                throw new ArgumentOutOfRangeException(nameof(bitwidth));

            hashFunctions = new CountSketchHashFunctions(bitwidth, rng);
            counters = new long[1UL << bitwidth];
        }

        public void Update(ulong x, long d)
        {
            var (hx, sx) = hashFunctions.Hashes(x);
            counters[hx] += sx * d;
        }

        public long EstimateF2()
        {
            return counters.Sum(c => c * c);
        }
    }
}
