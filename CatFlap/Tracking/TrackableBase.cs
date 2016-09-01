using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    public class TrackableBase : ITrackable
    {
        [Ignore]
        public string TrackingToken { get; set; }
    }
}
