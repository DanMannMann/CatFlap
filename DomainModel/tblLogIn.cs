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
    
    public partial class tblLogIn
    {
        public long ID { get; set; }
        public Nullable<long> AccountID { get; set; }
        public Nullable<long> SiteID { get; set; }
        public Nullable<bool> LogIn { get; set; }
        public Nullable<bool> Forced { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
    
        public virtual tblAccount tblAccount { get; set; }
        public virtual tblSite tblSite { get; set; }
    }
}