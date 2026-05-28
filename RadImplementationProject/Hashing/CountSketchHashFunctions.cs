using System;
using System.Numerics;

namespace RadImplementationProject.Hashing
{
    public class CountSketchHashFunctions
    {
        private readonly PolynomialHash g;
        private readonly int bitwidth;  // t, where m = 2^t
        private readonly ulong m;
        private readonly BigInteger twoTo88;

        public CountSketchHashFunctions(int bitwidth, Random rng)
        {
            if (bitwidth < 1 || bitwidth > 64)
                throw new ArgumentOutOfRangeException(nameof(bitwidth));

            this.bitwidth = bitwidth;
            this.m = 1UL << bitwidth;  // m = 2^t
            this.twoTo88 = BigInteger.One << 88;
            this.g = new PolynomialHash(bitwidth, rng);
        }

        public ulong H(ulong x)
        {
            // h(x) = g(x) mod m
            throw new NotImplementedException();
        }

        public int S(ulong x)
        {
            // s(x) = 1 - 2*floor(g(x) / 2^88)
            // Extracts the sign bit from g(x)
            throw new NotImplementedException();
        }

        public string Format()
        {
            return $"CountSketchHashFunctions(m=2^{bitwidth}, {g.Format()})";
        }
    }
}
