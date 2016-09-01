using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    [Serializable]
    public class TrackingEntry
    {
        public string ReferencePropertyName { get; set; }
        public TrackedPrimaryKey[][] TrackedEntities { get; set; }
    }
}
