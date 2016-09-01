using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class TrackAsAttribute : Attribute
    {
        public Type Type { get; private set; }

        public TrackAsAttribute(Type type)
        {
            Type = type;
        }
    }
}
