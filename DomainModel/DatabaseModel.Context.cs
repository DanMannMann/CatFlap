﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<tblAccount> tblAccounts { get; set; }
        public DbSet<tblAccountBackup> tblAccountBackups { get; set; }
        public DbSet<tblAccountBalance> tblAccountBalances { get; set; }
        public DbSet<tblAccountBalanceAudit> tblAccountBalanceAudits { get; set; }
        public DbSet<tblAccountChange> tblAccountChanges { get; set; }
        public DbSet<tblAccountDisclaimer> tblAccountDisclaimers { get; set; }
        public DbSet<tblAccountImage> tblAccountImages { get; set; }
        public DbSet<tblAccountNew> tblAccountNews { get; set; }
        public DbSet<tblAccountPreference> tblAccountPreferences { get; set; }
        public DbSet<tblAsset> tblAssets { get; set; }
        public DbSet<tblBalanceType> tblBalanceTypes { get; set; }
        public DbSet<tblBed> tblBeds { get; set; }
        public DbSet<tblBedType> tblBedTypes { get; set; }
        public DbSet<tblCabin> tblCabins { get; set; }
        public DbSet<tblCallRequest> tblCallRequests { get; set; }
        public DbSet<tblCashOut> tblCashOuts { get; set; }
        public DbSet<tblCashOutValue> tblCashOutValues { get; set; }
        public DbSet<tblCode> tblCodes { get; set; }
        public DbSet<tblComponent> tblComponents { get; set; }
        public DbSet<tblDisclaimer> tblDisclaimers { get; set; }
        public DbSet<tblFingerPrint> tblFingerPrints { get; set; }
        public DbSet<tblFloat> tblFloats { get; set; }
        public DbSet<tblFloatValue> tblFloatValues { get; set; }
        public DbSet<tblGiftCard> tblGiftCards { get; set; }
        public DbSet<tblImage> tblImages { get; set; }
        public DbSet<tblImageType> tblImageTypes { get; set; }
        public DbSet<tblImageUpload> tblImageUploads { get; set; }
        public DbSet<tblInfo> tblInfoes { get; set; }
        public DbSet<tblKioskInfo> tblKioskInfoes { get; set; }
        public DbSet<tblLogIn> tblLogIns { get; set; }
        public DbSet<tblOffer> tblOffers { get; set; }
        public DbSet<tblPreference> tblPreferences { get; set; }
        public DbSet<tblPreparedText> tblPreparedTexts { get; set; }
        public DbSet<tblReport> tblReports { get; set; }
        public DbSet<tblScript> tblScripts { get; set; }
        public DbSet<tblSecurityGroup> tblSecurityGroups { get; set; }
        public DbSet<tblSecurityPermission> tblSecurityPermissions { get; set; }
        public DbSet<tblSetting> tblSettings { get; set; }
        public DbSet<tblSettingsSymbol> tblSettingsSymbols { get; set; }
        public DbSet<tblSideBarMessage> tblSideBarMessages { get; set; }
        public DbSet<tblSignupInfo> tblSignupInfoes { get; set; }
        public DbSet<tblSite> tblSites { get; set; }
        public DbSet<tblSiteAd> tblSiteAds { get; set; }
        public DbSet<tblSiteGroup> tblSiteGroups { get; set; }
        public DbSet<tblSiteSetting> tblSiteSettings { get; set; }
        public DbSet<tblSkypeAccount> tblSkypeAccounts { get; set; }
        public DbSet<tblSoftwareUpdater> tblSoftwareUpdaters { get; set; }
        public DbSet<tblStaffLogin> tblStaffLogins { get; set; }
        public DbSet<tblTransaction> tblTransactions { get; set; }
        public DbSet<tblTransactionCabin> tblTransactionCabins { get; set; }
        public DbSet<tblTransactionCash> tblTransactionCashes { get; set; }
        public DbSet<tblTransactionGiftCard> tblTransactionGiftCards { get; set; }
        public DbSet<tblTransactionPayment> tblTransactionPayments { get; set; }
        public DbSet<tblTransactionTopup> tblTransactionTopups { get; set; }
        public DbSet<tblTransactionVending> tblTransactionVendings { get; set; }
        public DbSet<tblTransactionVoucher> tblTransactionVouchers { get; set; }
        public DbSet<tblUpdateLog> tblUpdateLogs { get; set; }
        public DbSet<tblVending> tblVendings { get; set; }
        public DbSet<tblVendItem> tblVendItems { get; set; }
        public DbSet<tblVersion> tblVersions { get; set; }
        public DbSet<tblVoucher> tblVouchers { get; set; }
    }
}
