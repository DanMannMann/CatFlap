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
    
    public partial class tblCode
    {
        public long ID { get; set; }
        public string VoucherCode { get; set; }
        public Nullable<bool> Used { get; set; }
        public Nullable<long> SiteID { get; set; }
    
        public virtual tblSite tblSite { get; set; }
    }
}
