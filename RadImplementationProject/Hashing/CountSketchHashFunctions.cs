using System;
using System.Numerics;

namespace RadImplementationProject.Hashing
{
    public class CountSketchHashFunctions
    {
        private readonly PolynomialHash g;
        private readonly int bitwidth;  // t, where m = 2^t
        private readonly ulong m;

        public CountSketchHashFunctions(int t, Random rng)
        {
            if (bitwidth < 1 || bitwidth > 64)
                throw new ArgumentOutOfRangeException(nameof(bitwidth));

            bitwidth = t;
            m = 1UL << t;
            g = new PolynomialHash(t, rng);
        }

        public (ulong, int) Hashes(ulong x)
        {
            var gx = g.Hash(x);
            var hx = gx & (m - 1);
            var bx = (int)(gx >> (g.primeExp - 1));
            var sx = 1 - 2*bx;
            return (hx, sx);
        }
        public string Format()
        {
            return $"CountSketchHashFunctions(m=2^{bitwidth}, {g.Format()})";
        }
    }
}
