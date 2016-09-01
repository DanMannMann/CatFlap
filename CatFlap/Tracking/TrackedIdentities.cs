using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class TrackedIdentities : List<TrackedIdentity>
    {
        public static TrackedIdentities FromArray(TrackedPrimaryKey[][] array)
        {
            TrackedIdentities result = new TrackedIdentities();
            foreach (var entry in array)
            {
                result.Add(TrackedIdentity.FromArray(entry));
            }
            return result;
        }

        public TrackedPrimaryKey[][] ToArray()
        {
            var result = new TrackedPrimaryKey[this.Count][];
            int i = 0;
            foreach (var item in this)
            {
                result[i] = item.ToArray();
                i++;
            }
            return result;
        }
    }
}
