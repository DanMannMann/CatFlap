//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DomainModel
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblFloatValue
    {
        public long ID { get; set; }
        public Nullable<long> FloatID { get; set; }
        public Nullable<int> Value { get; set; }
        public Nullable<int> Amount { get; set; }
        public Nullable<bool> Old { get; set; }
    
        public virtual tblFloat tblFloat { get; set; }
    }
}
