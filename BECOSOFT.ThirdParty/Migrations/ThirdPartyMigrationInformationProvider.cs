using BECOSOFT.Data.Migrator;
using BECOSOFT.ThirdParty.Migrations.Attributes;
using System;
using System.Collections.Generic;

namespace BECOSOFT.ThirdParty.Migrations {
    public class ThirdPartyMigrationInformationProvider : IBaseMigrationInformationProvider<ThirdPartyMigrationType> {
        public string TableName => "ThirdPartyMigrations";
        public string TableNameDynamic => "ThirdPartyMigrationsDynamic";
        public string SchemaName => "thirdparty";
        public Type DescriptionAttributeType => null;
        public List<Type> MigrationAttributes => new List<Type> { typeof(VatNumberDetailsMigrationAttribute) };
        public Dictionary<ThirdPartyMigrationType, List<ThirdPartyMigrationType>> LinkedMigrationTypes => null;

        public bool ShouldRegisterVersion(ThirdPartyMigrationType migrationType) => true;
        public Type GetMigrationClassType(ThirdPartyMigrationType migrationType) => typeof(BaseMigration<ThirdPartyMigrationType>);
        public bool IsDynamic(ThirdPartyMigrationType migrationType) => false;
    }
}
