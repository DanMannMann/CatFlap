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
    
    public partial class tblAccountBalance
    {
        public tblAccountBalance()
        {
            this.tblAccountBalanceAudits = new HashSet<tblAccountBalanceAudit>();
        }
    
        public long ID { get; set; }
        public Nullable<long> BalanceTypeID { get; set; }
        public Nullable<long> AccountID { get; set; }
        public Nullable<decimal> Balance { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    
        public virtual tblAccount tblAccount { get; set; }
        public virtual tblBalanceType tblBalanceType { get; set; }
        public virtual ICollection<tblAccountBalanceAudit> tblAccountBalanceAudits { get; set; }
    }
}
