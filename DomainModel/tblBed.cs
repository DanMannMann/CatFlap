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
    
    public partial class tblBed
    {
        public tblBed()
        {
            this.tblAssets = new HashSet<tblAsset>();
            this.tblCabins = new HashSet<tblCabin>();
        }
    
        public long ID { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public Nullable<long> BedTypeID { get; set; }
        public Nullable<long> ImageID { get; set; }
        public Nullable<int> UndressTime { get; set; }
        public Nullable<int> DressTime { get; set; }
        public Nullable<bool> Over18 { get; set; }
        public bool Deleted { get; set; }
    
        public virtual ICollection<tblAsset> tblAssets { get; set; }
        public virtual tblBedType tblBedType { get; set; }
        public virtual tblImage tblImage { get; set; }
        public virtual ICollection<tblCabin> tblCabins { get; set; }
    }
}