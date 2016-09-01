using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    [Serializable]
    public class TrackedPrimaryKey
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
    }
}
