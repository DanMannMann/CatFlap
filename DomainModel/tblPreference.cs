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
    
    public partial class tblPreference
    {
        public tblPreference()
        {
            this.tblAccountPreferences = new HashSet<tblAccountPreference>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<tblAccountPreference> tblAccountPreferences { get; set; }
    }
}