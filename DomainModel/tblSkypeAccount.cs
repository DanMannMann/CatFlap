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
    
    public partial class tblSkypeAccount
    {
        public long ID { get; set; }
        public Nullable<long> AccountSiteID { get; set; }
        public string SkypeName { get; set; }
        public string SkypePassword { get; set; }
        public Nullable<bool> Site { get; set; }
    }
}
