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
    
    public partial class tblCabin
    {
        public tblCabin()
        {
            this.tblTransactionCabins = new HashSet<tblTransactionCabin>();
        }
    
        public long ID { get; set; }
        public Nullable<byte> CabinNumber { get; set; }
        public Nullable<long> SiteID { get; set; }
        public Nullable<long> BedID { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<long> AssetID { get; set; }
        public bool Deleted { get; set; }
    
        public virtual tblAsset tblAsset { get; set; }
        public virtual tblBed tblBed { get; set; }
        public virtual tblSite tblSite { get; set; }
        public virtual ICollection<tblTransactionCabin> tblTransactionCabins { get; set; }
    }
}