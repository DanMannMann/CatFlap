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
    
    public partial class tblGiftCard
    {
        public tblGiftCard()
        {
            this.tblTransactionGiftCards = new HashSet<tblTransactionGiftCard>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> Value { get; set; }
        public Nullable<long> ImageID { get; set; }
        public bool Deleted { get; set; }
    
        public virtual tblImage tblImage { get; set; }
        public virtual ICollection<tblTransactionGiftCard> tblTransactionGiftCards { get; set; }
    }
}
