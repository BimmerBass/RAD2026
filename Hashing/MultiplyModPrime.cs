using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Hashing
{
    public class MultiplyModPrime : IHashFunction
    {
        const int q = 89;
        readonly BigInteger _p = BigInteger.Pow(2, q) - 1;
        readonly BigInteger _a, _b;
        int _l;
        public MultiplyModPrime(int l, Random rng)
        {
            // Since p is just the first 89 bytes being 1, we can generate a and b to be below p
            // by masking with p
            _a = rng.BigIntegerMasked(_p);
            _b = rng.BigIntegerMasked(_p);
            SetBitwidth(l);

            Debug.Assert(_a != 0 && _a < _p);
            Debug.Assert(_b < _p);
        }

        public ulong Hash(ulong x)
        {
            var bigx = new BigInteger(x);
            var inner = _a * bigx + _b;
            var modp = ModuloP(inner);

            var mask = (1UL << _l) - 1;
            return (ulong)(modp & mask);
        }

        public void SetBitwidth(int newL)
        {
            _l = newL;
            Debug.Assert(_l > 0 && _l < 64);
        }
        public ulong Range() => 1UL << _l;

        public string Format()
        {
            return $"h(x) = ((0x{_a:x} * x + 0x{_b:x}) mod 0x{_p:x}) mod 2^{_l}";
        }

        private BigInteger ModuloP(BigInteger x)
        {
            var y = (x & _p) + (x >> q);
            if (y >= _p) y -= _p;
            return y;
        }
    }
}
