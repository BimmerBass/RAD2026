using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Hashing
{
    public class MultiplyShift : IHashFunction
    {
        readonly ulong _a;
        int _l; // important that its unsigned -> performs logical shift instead of arithmetic
        public MultiplyShift(int l, Random rng)
        {
            // Could've used random.org
            // however we chose to randomly generate 8 bytes ourselves and set the LSB = 1
            var aBytes = rng.GetBytes(8);
            _a = BitConverter.ToUInt64(aBytes) | 1UL;
            SetBitwidth(l);


            Debug.Assert(_a % 2 != 0);
        }

        public ulong Hash(ulong x)
        {
            // overflow is intended.
            return unchecked((_a * x) >> (64 - _l));
        }

        public void SetBitwidth(int newL)
        {
            _l = newL;
            Debug.Assert(_l > 0 && _l < 64);
        }
        public ulong Range() => 1UL << _l;

        public string Format()
        {
            return $"h(x) = (0x{_a:x} * x) >> (64 - {_l})";
        }
    }
}
