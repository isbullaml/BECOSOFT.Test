using BECOSOFT.Data.Migrator.Attributes;

namespace BECOSOFT.ThirdParty.Migrations.Attributes {
    public class VatNumberDetailsMigrationAttribute : BaseMigrationAttribute {
        protected override object MigrationTypeValue => ThirdPartyMigrationType.VatNumberDetails;

        public VatNumberDetailsMigrationAttribute(int version, string information) : base(version, information) { }
    }
}
