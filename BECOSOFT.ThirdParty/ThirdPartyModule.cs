using Autofac;
using BECOSOFT.Data;
using BECOSOFT.Data.Migrator.Services.Interfaces;
using BECOSOFT.ThirdParty.Migrations;

namespace BECOSOFT.ThirdParty {
    public class ThirdPartyModule : BaseModule<ThirdPartyBuildOptions> {
        public static IContainer Build(ThirdPartyBuildOptions buildOptions) {
            return ModuleBuilder.BuildWithoutConnection<ThirdPartyModule, ThirdPartyBuildOptions>(buildOptions);
        }

        protected internal override void HandleBuild(ILifetimeScope kernel, ContainerBuilder containerBuilder, ThirdPartyBuildOptions dataBuildOptions) {
            var migrationService = kernel.Resolve<IBaseMigrationService<ThirdPartyMigrationType>>();
            migrationService.Upgrade(ThirdPartyMigrationType.VatNumberDetails);
        }
    }

    public class ThirdPartyBuildOptions : BaseBuildOptions {
        public ThirdPartyBuildOptions(string connection) : base(connection) { }
        public ThirdPartyBuildOptions() : base() { }
    }
}
