using System;
using System.Numerics;

namespace RadImplementationProject.Hashing
{
    public class PolynomialHash : IHashFunction<BigInteger>
    {
        private readonly BigInteger a0;
        private readonly BigInteger a1;
        private readonly BigInteger a2;
        private readonly BigInteger a3;

        public readonly int primeExp;
        public readonly BigInteger prime;  // p = 2^89 - 1

        public PolynomialHash(Random rng)
        {
            // p = 2^89 - 1
            primeExp = 89;
            prime = (BigInteger.One << primeExp) - 1;
            
            // Initialize random coefficients in [p] using masked random generation
            a0 = rng.BigIntegerMasked(prime);
            a1 = rng.BigIntegerMasked(prime);
            a2 = rng.BigIntegerMasked(prime);
            a3 = rng.BigIntegerMasked(prime);
        }

        public BigInteger Hash(BigInteger x)
        {
            const int q = 4;
            var a = new[] { a0, a1, a2, a3 };
            var y = a[q-1];
            for (var i = q - 2; i >= 0; i--)
            {
                y = y * x + a[i];
                y = (y & prime) + (y >> primeExp);
            }
            if (y >= prime)
            {
                y -= prime;
            }
            return y;

            /*var coefficients = new[] { a2, a0, a1 };
            var y = a3;
            for (int i = 2; i >= 0; i--)
            {
                y = y * x + coefficients[i];
                y = y & prime + y >> 89; // mod p
            }
            if (y >= prime) y -= prime;
            return y;*/
        }

        public string Format()
        {
            return $"PolynomialHash(p=2^89-1)";
        }

        public void SetBitwidth(int newL)
        {
            throw new NotImplementedException();
        }

        public BigInteger Range()
        {
            throw new NotImplementedException();
        }
    }
}
