using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FelineSoft.CatFlap
{
    internal class ReattachJob
    {
        public PropertyInfo Property { get; set; }
        public object TargetInstance { get; set; }
        public object Value { get; set; }
        public Type SourceType { get; set; }
        public Type TargetType { get; set; }
        public RelationshipType RelationshipType { get; set; }
        public object SourceInstance { get; set; }
    }
}
