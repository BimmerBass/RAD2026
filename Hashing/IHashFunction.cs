using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Hashing
{
    public interface IHashFunction
    {
        ulong Hash(ulong x);
        string Format();
        void SetBitwidth(int newL);
        ulong Range();
    }
}
