using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RadImplementationProject.Util
{
    public class ListTypeConverter : TypeConverter
    {
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value == null)
                return null;

            if (value is string sv)
            {
                var parts = sv
                    .Split(',')
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Where(x => int.TryParse(x, out var _))
                    .Select(int.Parse)
                    .ToList();
                return parts;
            }
            throw new InvalidOperationException("expected string");
        }
    }
}
