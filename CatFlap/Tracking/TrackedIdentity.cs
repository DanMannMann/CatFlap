using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class TrackedIdentity : List<TrackedPrimaryKey>
    {
        public static TrackedIdentity FromArray(TrackedPrimaryKey[] array)
        {
            TrackedIdentity result = new TrackedIdentity();
            foreach (var entry in array)
            {
                result.Add(entry);
            }
            return result;
        }

        public TrackedPrimaryKey[] ToArray()
        {
            return base.ToArray();
        }
    }
}
