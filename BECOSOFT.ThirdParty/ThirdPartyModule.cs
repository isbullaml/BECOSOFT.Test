using Autofac;
using BECOSOFT.Data;

namespace BECOSOFT.ThirdParty {
    public class ThirdPartyModule : BaseModule<ThirdPartyBuildOptions> {
        public static IContainer Build(ThirdPartyBuildOptions buildOptions) {
            return ModuleBuilder.BuildWithoutConnection<ThirdPartyModule, ThirdPartyBuildOptions>(buildOptions);
        }
    }

    public class ThirdPartyBuildOptions : BaseBuildOptions {
        public ThirdPartyBuildOptions(string connection) : base(connection) { }
        public ThirdPartyBuildOptions() : base() { }
    }
}
