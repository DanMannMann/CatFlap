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
    
    public partial class tblSiteSetting
    {
        public System.Guid ID { get; set; }
        public Nullable<System.Guid> SymbolID { get; set; }
        public Nullable<long> SiteID { get; set; }
        public string Value { get; set; }
    
        public virtual tblSettingsSymbol tblSettingsSymbol { get; set; }
        public virtual tblSite tblSite { get; set; }
    }
}
