using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject
{
    public static class Extensions
    {
        public static readonly int SEED = 0x1234;
        public static byte[] GetBytes(this Random rng, uint nBytes)
        {
            var bytes = new byte[nBytes];
            rng.NextBytes(bytes);
            return bytes;
        }

        public static BigInteger BigIntegerMasked(this Random rng, BigInteger mask)
        {
            var bytes = rng.GetBytes(128 / 8);
            var result = new BigInteger(bytes, isUnsigned: true);
            return result & mask;
        }

        public static T AddAndReturn<T>(this IList<T> list, T item)
        {
            list.Add(item);
            return item;
        }
    }
}
