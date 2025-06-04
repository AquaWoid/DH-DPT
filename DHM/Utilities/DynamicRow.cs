using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHM.Utilities
{
    public class DynamicRow : DynamicObject
    {
        public List<string> Values { get; }

        public DynamicRow(List<string> values)
        {
            Values = values;
        }
    }
}
