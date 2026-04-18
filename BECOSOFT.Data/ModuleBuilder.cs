using Autofac;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Utilities.Exceptions;

namespace BECOSOFT.Data {
    public static class ModuleBuilder {
        /// <summary>
        /// Build the <see cref="IContainer"/> according to the specified <see cref="dataBuildOptions"/>.
        /// </summary>
        /// <param name="dataBuildOptions"><see cref="BaseBuildOptions"/> that specify extra options to execute after building the <see cref="IContainer"/>.</param>
        /// <exception cref="DbConnectionException">Throws an exception when the <see cref="BaseBuildOptions.Connection"/> (<see cref="BaseBuildOptions"/>) is not a valid connection string</exception>
        /// <returns></returns>
        public static IContainer Build<T, TBuildOptions>(TBuildOptions dataBuildOptions)
            where T : BaseModule<TBuildOptions>, new() where TBuildOptions : BaseBuildOptions {
            if (!SqlConnectionHelper.IsValid(dataBuildOptions.Connection)) {
                throw new DbConnectionException(Resources.Error_Connection_Invalid);
            }

            return BuildInternal<T, TBuildOptions>(dataBuildOptions);
        }

        public static IContainer BuildWithoutConnection<T, TBuildOptions>(TBuildOptions dataBuildOptions)
            where T : BaseModule<TBuildOptions>, new() where TBuildOptions : BaseBuildOptions {
            return BuildInternal<T, TBuildOptions>(dataBuildOptions);
        }

        public static void RegisterTo<T, TBuildOptions>(ContainerBuilder containerBuilder, TBuildOptions dataBuildOptions) where T : BaseModule<TBuildOptions>, new() where TBuildOptions : BaseBuildOptions {
            RegisterToWithoutConnection<T, TBuildOptions>(containerBuilder, dataBuildOptions);
            BaseModule<TBuildOptions>.RegisterConnection(containerBuilder, dataBuildOptions.Connection);
        }

        public static void RegisterToWithoutConnection<T, TBuildOptions>(ContainerBuilder containerBuilder, TBuildOptions dataBuildOptions) where T : BaseModule<TBuildOptions>, new() where TBuildOptions : BaseBuildOptions {
            var coreModule = new T {
                BuildOptions = dataBuildOptions
            };

            coreModule.AddAssembliesToLoad(dataBuildOptions.AssembliesToLoad);
            coreModule.RegisterTo(containerBuilder);
            BaseModule<TBuildOptions>.RegisterConnection(containerBuilder, dataBuildOptions.Connection);
        }

        private static IContainer BuildInternal<T, TBuildOptions>(TBuildOptions dataBuildOptions)
            where T : BaseModule<TBuildOptions>, new() where TBuildOptions : BaseBuildOptions {
            var module = new T {
                BuildOptions = dataBuildOptions
            };

            module.AddAssembliesToLoad(dataBuildOptions.AssembliesToLoad);

            var containerBuilder = module.GetContainerBuilder(dataBuildOptions);
            BaseModule<TBuildOptions>.RegisterConnection(containerBuilder, dataBuildOptions.Connection);
            var kernel = containerBuilder.Build();

            module.HandleBuild(kernel, containerBuilder, dataBuildOptions);

            return kernel;
        }
    }
}