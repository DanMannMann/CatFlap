using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FelineSoft.CatFlap
{
    public class MapToAttribute : Attribute
    {
        public MapToAttribute(Type entityType, string propertyName)
        {
            EntityType = entityType;
            PropertyName = propertyName;
        }

        public Type EntityType { get; set; }
        public string PropertyName { get; set; }
    }
}
