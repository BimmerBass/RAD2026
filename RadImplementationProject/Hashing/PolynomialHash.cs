using System;
using System.Numerics;

namespace RadImplementationProject.Hashing
{
    public class PolynomialHash : IHashFunction
    {
        private readonly BigInteger a0;
        private readonly BigInteger a1;
        private readonly BigInteger a2;
        private readonly BigInteger a3;
        
        private readonly BigInteger prime=BigInteger.Pow(2, 89) - 1;  // p = 2^89 - 1
        private int bitwidth;

        public PolynomialHash(int bitwidth, Random rng)
        {
            if (bitwidth < 1 || bitwidth > 64)
                throw new ArgumentOutOfRangeException(nameof(bitwidth));

            this.bitwidth = bitwidth;
            
            // p = 2^89 - 1
            this.prime = (BigInteger.One << 89) - 1;
            
            // Initialize random coefficients in [p] using masked random generation
            a0 = rng.BigIntegerMasked(prime);
            a1 = rng.BigIntegerMasked(prime);
            a2 = rng.BigIntegerMasked(prime);
            a3 = rng.BigIntegerMasked(prime);



        }

        public ulong Hash(ulong x)
        {
            var y = a3;
            for (int i = 2; i >= 0; i--)
            {
                y = y * x + GetCoefficient(i);
                y = y & prime + y >> 89; // mod p
            }
            if (y >= prime) y -= prime;
            return y;
        }

        public string Format()
        {
            return $"PolynomialHash(p=2^89-1, bitwidth={bitwidth})";
        }

        public void SetBitwidth(int newL)
        {
            if (newL < 1 || newL > 64)
                throw new ArgumentOutOfRangeException(nameof(newL));
            bitwidth = newL;
        }

        
    }
}
