using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Util
{
    public class QuadraticSumRow
    {
        [Name("l-sample")]
        [NameIndex(0)]
        public int LValue { get; set; }
        [Name("multiply-shift-ms")]
        [NameIndex(1)]
        public double MultiplyShiftMs { get; set; }
        [Name("multiply-mod-prime-ms")]
        [NameIndex(2)]
        public double MultiplyModPrimeMs { get; set; }
        [Name("second-moment")]
        [NameIndex(3)]
        public long SecondMoment { get; set; }
    }
}
