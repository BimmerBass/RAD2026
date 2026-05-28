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

            this.hashFunctions = new CountSketchHashFunctions(bitwidth, rng);
            this.counters = new long[1UL << bitwidth];
        }

        public void Update(ulong x, long d)
        {
            // Update operation: C[h(x)] += s(x) * d
            throw new NotImplementedException();
        }

        public long Estimate()
        {
            // sum of C[i]^2 for all i in [m]
            return counters.Sum(c => c * c);
        }
    }
}
