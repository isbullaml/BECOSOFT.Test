using BECOSOFT.Data.Migrator;
using BECOSOFT.Data.Migrator.Services;
using System;

namespace BECOSOFT.ThirdParty.Migrations {
    public class ThirdPartyMigrationService : BaseMigrationService<ThirdPartyMigrationType> {
        public ThirdPartyMigrationService(Lazy<IDatabaseMigratorFactory<ThirdPartyMigrationType>> migratorFactory,
                                          IBaseMigrationInformationProvider<ThirdPartyMigrationType> informationProvider)
            : base(migratorFactory, informationProvider) { }
    }
}
