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
    
    public partial class tblAccountBackup
    {
        public long ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Number { get; set; }
        public string Email { get; set; }
        public string PostCode { get; set; }
        public string PinNumber { get; set; }
        public Nullable<long> SiteID { get; set; }
        public Nullable<bool> Female { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public Nullable<bool> Suspended { get; set; }
        public string SuspendMessage { get; set; }
    }
}