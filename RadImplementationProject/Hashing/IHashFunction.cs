using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Hashing
{
    public interface IHashFunction<T>
    {
        T Hash(T x);
        string Format();
        void SetBitwidth(int newL);
        T Range();
    }
    public interface IHashFunction : IHashFunction<ulong> { }
}
