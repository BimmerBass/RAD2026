using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Util
{
    public class CountSketchEstimate
    {
        [Name("index")]
        [NameIndex(0)]
        public int Index { get; set; }

        [Name("raw_value")]
        [NameIndex(1)]
        public long Estimate { get; set; } // X_i

        [Name("sorted_value")]
        [NameIndex(2)]
        public long SortedEstimate { get; set; } // X_(i)

        [Name("exact")]
        [NameIndex(3)]
        public long ExactF2 { get; set; }

        [Name("mse")]
        [NameIndex(4)]
        public double MeanSquareError { get; set; }
    }
}
