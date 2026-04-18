#region usings

using Autofac;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using BECOSOFT.Data.Caching.Interfaces;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Context.Remote;
using BECOSOFT.Data.Migrator;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Optimizer;
using BECOSOFT.Data.RemotePowershell;
using BECOSOFT.Data.Repositories;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Repositories.QueryData;
using BECOSOFT.Data.Services;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Data.Validation;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using BECOSOFT.Utilities.Cache;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Helpers;
using BECOSOFT.Utilities.Security.OAuth;
using System.Collections.Concurrent;
using Module = Autofac.Module;

#endregion

namespace BECOSOFT.Data {
    /// <inheritdoc />
    public abstract class BaseModule<TBuildOptions> : Module where TBuildOptions : BaseBuildOptions {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        internal TBuildOptions BuildOptions;
        private readonly List<string> _assembliesToLoad;
        private readonly List<string> _partialAssemblyNamesToIgnore = FillPartialAssemblyNamesToIgnore();
        protected Assembly[] LoadableAssemblies;
        protected IReadOnlyList<Type> LoadableAssemblyTypes;

        private readonly ConcurrentDictionary<Type, Tuple<PropertyInfo, bool>> _cacheableTypes = new ConcurrentDictionary<Type, Tuple<PropertyInfo, bool>>();
        private bool _hasCachableTypes;
        private Dictionary<Assembly, List<Type>> _assemblyLoadableTypes = new Dictionary<Assembly, List<Type>>();

        /// <summary>
        /// <see cref="BaseModule{TBuildOptions}"/> constructor
        /// </summary>
        protected BaseModule() {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => {
                var ex = args.ExceptionObject as Exception;
                if (args.IsTerminating) {
                    Logger.Fatal(ex, $"Terminating due to unhandled exception, sender: {sender}");
                } else {
                    Logger.Fatal(ex, $"Sender: {sender}");
                }
            };
            var entryAssembly = Assembly.GetEntryAssembly()?.FullName;
            _assembliesToLoad = new List<string> { "BECOSOFT".ToLowerInvariant(), };
            if (entryAssembly != null) { _assembliesToLoad.Add(entryAssembly.ToLowerInvariant()); }
            LoadableAssemblies = Array.Empty<Assembly>();
            LoadableAssemblyTypes = new List<Type>(0);
            UpdateSecurityProtocol();
        }

        internal void AddAssembliesToLoad(List<string> assembliesToLoad) {
            if (assembliesToLoad.IsEmpty()) { return; }
            foreach (var assemblyToLoad in assembliesToLoad) {
                _assembliesToLoad.Add(assemblyToLoad.ToLowerInvariant());
            }
        }

        internal ContainerBuilder GetContainerBuilder(TBuildOptions buildOptions) {
            var containerBuilder = new ContainerBuilder();
            BaseRegisterTo(containerBuilder);
            return containerBuilder;
        }

        /// <summary>
        /// Function that occurs after the build.
        /// Override this function to perform actions right after the build finished.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="containerBuilder"></param>
        /// <param name="dataBuildOptions"></param>
        protected internal virtual void HandleBuild(ILifetimeScope kernel, ContainerBuilder containerBuilder, TBuildOptions dataBuildOptions) {
        }

        /// <summary>
        /// Function that occurs before the build.
        /// Override this function to perform actions right before the build starts.
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <param name="buildOptions"></param>
        protected virtual void HandleGetContainerBuilder(ContainerBuilder containerBuilder, TBuildOptions buildOptions) {
        }

        /// <summary>
        /// Function that occurs at the end of the loading.
        /// Override this function to perform extra loading actions.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        /// <param name="buildOptions"></param>
        protected virtual void HandleLoad(ContainerBuilder builder, Assembly[] assemblies, TBuildOptions buildOptions) {
        }

        internal void RegisterTo(ContainerBuilder builder) {
            BaseRegisterTo(builder);
            builder.RegisterBuildCallback(c => {
                HandleBuild(c, builder, BuildOptions);
            });
        }

        private void BaseRegisterTo(ContainerBuilder builder) {
            builder.ComponentRegistryBuilder.Registered += (sender, args) => {
                args.ComponentRegistration.PipelineBuilding += (sender2, pipeline) => {
                    pipeline.Use(PipelinePhase.Activation, MiddlewareInsertionMode.EndOfPhase, (c, next) => {
                        next(c);
                        InjectIsCacheEnabledProperty(sender, c.Instance);
                    });
                };
            };
            try {
                LoadAssemblyAndTypes();
            } catch (ReflectionTypeLoadException ex) {
                Logger.Error(ex);
                if (ex.LoaderExceptions.IsEmpty()) {
                    throw;
                }
                foreach (var loaderException in ex.LoaderExceptions) {
                    Logger.Error(loaderException);
                }

                throw;
            } catch (Exception ex) {
                Logger.Error(ex);

                throw;
            }
            builder.RegisterModule(this);

            HandleGetContainerBuilder(builder, BuildOptions);

            if (BuildOptions.BuilderAction != null) {
                BuildOptions.BuilderAction(builder);
            }
        }

        /// <inheritdoc />
        protected override void Load(ContainerBuilder builder) {
            try {
                LoadGeneral(builder);
                LoadMigrations(builder);
                LoadOptimizations(builder);
                LoadContextFactories(builder);
                LoadRepositories(builder);
                LoadValidators(builder);
                LoadServices(builder);
                LoadGeneric(builder);
                LoadFactories(builder);
                LoadOAuthClients(builder);
                LoadThirdPartyFactories(builder);
                HandleLoad(builder, LoadableAssemblies, BuildOptions);
            } catch (ReflectionTypeLoadException ex) {
                Logger.Error(ex);
                if (ex.LoaderExceptions.IsEmpty()) {
                    throw;
                }
                foreach (var loaderException in ex.LoaderExceptions) {
                    Logger.Error(loaderException);
                }

                throw;
            } catch (Exception ex) {
                Logger.Error(ex);

                throw;
            } finally {
                _assemblyLoadableTypes = new Dictionary<Assembly, List<Type>>(0);
            }
        }

        private void LoadAssemblyAndTypes() {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var currentlyLoaded = allAssemblies.Select(a => a.GetName().FullName.ToLowerInvariant()).ToHashSet();
            var allAssembliesWithReferenced = allAssemblies.SelectMany(x => x.GetReferencedAssemblies()).Distinct();
            var allAssembliesWithLowerCaseNames = allAssembliesWithReferenced.Select(a => (Assembly: a, LowerFullName: a.FullName.ToLowerInvariant())).ToList();
            var toLoadInDomain = allAssembliesWithLowerCaseNames.Where(a => !currentlyLoaded.Contains(a.LowerFullName) && LoadableAssemblyFilter(a.LowerFullName)).ToList();
            foreach (var assemblyInfo in toLoadInDomain) {
                try {
                    var assembly = AppDomain.CurrentDomain.Load(assemblyInfo.Assembly);
                    allAssemblies.Add(assembly);
                } catch (Exception e) {
                    Logger.Trace("{0} not found (could not be loaded)", assemblyInfo.Assembly);
                    Logger.Trace(e);
                }
            }

            LoadableAssemblies = allAssemblies.Where(a => AssemblyFilter(a.GetName().FullName.ToLowerInvariant())).ToArray();
            LoadableAssemblyTypes = LoadableAssemblies.SelectMany(GetLoadableTypesForAssembly).ToList();
        }

        private bool LoadableAssemblyFilter(string lowerCaseAssemblyFullName) {
            if (AssemblyFilter(lowerCaseAssemblyFullName)) {
                return true;
            }
            for (var i = 0; i < _partialAssemblyNamesToIgnore.Count; i++) {
                var partial = _partialAssemblyNamesToIgnore[i];
                if (lowerCaseAssemblyFullName.Contains(partial)) {
                    return false;
                }
            }
            return true;
        }

        private static List<string> FillPartialAssemblyNamesToIgnore() {
            var items = new List<string> {
                "System",
                "mscor",
                "Microsoft",
                "Autofac",
                "NLog",
                "Newtonsoft",
                "CrystalDecisions.",
                "BenchmarkDot",
                "Infragistics",
                "WebGrease",
                "Presentation.",
                "Mono.",
                "BECOCRM",
                "CefSharp",
                "r4nd_class",
                "UnityEngine",
                "MiniProfiler",
                "Bambora",
                "EnvDTE",
                "Magick",
            };

            return items.Select(i => i.ToLowerInvariant()).ToList();
        }

        private bool AssemblyFilter(string lowerCaseAssemblyFullName) {
            for (var i = 0; i < _assembliesToLoad.Count; i++) {
                var toLoad = _assembliesToLoad[i];
                if (lowerCaseAssemblyFullName.Contains(toLoad)) {
                    return true;
                }
            }
            return false;
        }

        private static void UpdateSecurityProtocol() {
            var secProtocols = ServicePointManager.SecurityProtocol;
            var newSecurityProtocol = secProtocols | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            if (secProtocols == newSecurityProtocol) {
                return;
            }
            Logger.Info("Update {0} from {1} to {2}", nameof(ServicePointManager.SecurityProtocol), secProtocols, newSecurityProtocol);
            ServicePointManager.SecurityProtocol = newSecurityProtocol;
        }

        private void LoadGeneric(ContainerBuilder builder) {
            var baseEntityType = typeof(BaseEntity);
            var types = LoadableAssemblyTypes.Where(t => !t.IsAbstract && !t.IsInterface && baseEntityType.IsAssignableFrom(t)).ToList();
            LoadGenericRepositories(builder, types);
            var validatableType = typeof(IValidatable);
            var validatorTypes = LoadableAssemblyTypes.Where(t => !t.IsAbstract && !t.IsInterface && validatableType.IsAssignableFrom(t)).ToList();
            LoadGenericValidators(builder, validatorTypes);
            LoadGenericServices(builder, types);
        }

        private void LoadOAuthClients(ContainerBuilder builder) {
            var oAuthInterfaceType = typeof(IOAuthClient<,>);
            var oAuthType = typeof(OAuthClient<,>);
            var types = LoadableAssemblyTypes.Where(t => !t.IsInterface && !t.IsAbstract && t.IsSubclassOfRawGeneric(oAuthType)).ToList();
            foreach (var implementedType in types) {
                var abstr = implementedType.BaseType;
                var interf = abstr?.GetInterfaces().FirstOrDefault(t => t.GetGenericTypeDefinition() == oAuthInterfaceType);
                if (interf == null) { continue; }
                builder.RegisterType(implementedType).As(interf);
            }
        }

        private void LoadGenericServices(ContainerBuilder builder, List<Type> types) {
            var tableConsumingEntityType = typeof(TableConsumingEntity<>);
            var tableConsumingTranslateableEntityType = typeof(TableConsumingTranslateableEntity<,>);

            var serviceType = typeof(Service<>);
            var tableService = typeof(TableService<,>);
            var concreteImplementedTypes = LoadableAssemblyTypes.Where(t => !t.IsGenericType && (t.IsSubclassOfRawGeneric(serviceType)
                                                                                                 || t.IsSubclassOfRawGeneric(tableService)))
                                                                .Select(t => t.BaseType?.GenericTypeArguments.FirstOrDefault())
                                                                .Where(t => t != null).ToSafeHashSet();
            var typesToAdd = types.Where(t => !concreteImplementedTypes.Contains(t)).ToList();

            var genericServiceType = typeof(GenericService<>);
            var genericTableServiceType = typeof(GenericTableService<,>);
            var serviceInterfaceType = typeof(IService<>);
            var tableServiceInterfaceType = typeof(ITableService<,>);

            foreach (var type in typesToAdd) {
                Type concreteType;
                Type interfaceType;
                if (type.IsSubclassOfRawGeneric(tableConsumingTranslateableEntityType)) {
                    Type definingType = null;
                    var tempType = type.BaseType;

                    while (tempType?.BaseType != null && tempType.GenericTypeArguments.Length < 2) {
                        tempType = tempType.BaseType;
                    }

                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments[1];
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableServiceType.MakeGenericType(type, definingType);
                    interfaceType = tableServiceInterfaceType.MakeGenericType(type, definingType);
                } else if (type.IsSubclassOfRawGeneric(tableConsumingEntityType)) {
                    Type definingType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableServiceType.MakeGenericType(type, definingType);
                    interfaceType = tableServiceInterfaceType.MakeGenericType(type, definingType);
                } else {
                    concreteType = genericServiceType.MakeGenericType(type);
                    interfaceType = serviceInterfaceType.MakeGenericType(type);
                }
                builder.RegisterType(concreteType)
                       .As(interfaceType)
                       .FindConstructorsWith(InternalConstructorFinder);
            }
        }

        private void LoadGenericValidators(ContainerBuilder builder, List<Type> types) {
            var tableConsumingEntityType = typeof(TableConsumingEntity<>);
            var translateableEntityType = typeof(TranslateableEntity<>);
            var tableConsumingTranslateableEntityType = typeof(TableConsumingTranslateableEntity<,>);
            var tableConsumingTranslationEntityType = typeof(TableConsumingTranslationEntity<>);

            var validatorType = typeof(Validator<>);
            var translationValidatorType = typeof(Validator<,>);
            var tableConsumingValidatorType = typeof(TableConsumingValidator<,>);
            var tableConsumingTranslateableValidatorType = typeof(TableConsumingTranslateableValidator<,,>);
            var concreteImplementedTypes = LoadableAssemblyTypes.Where(t => !t.IsGenericType && (t.IsSubclassOfRawGeneric(validatorType)
                                                                                                 || t.IsSubclassOfRawGeneric(translationValidatorType)
                                                                                                 || t.IsSubclassOfRawGeneric(tableConsumingValidatorType)
                                                                                                 || t.IsSubclassOfRawGeneric(tableConsumingTranslateableValidatorType)))
                                                                .Select(t => t.BaseType?.GenericTypeArguments.FirstOrDefault())
                                                                .Where(t => t != null).ToSafeHashSet();
            var typesToAdd = types.Where(t => !concreteImplementedTypes.Contains(t)).ToList();
            var validatorInterface = typeof(IValidator<>);
            var tableConsumingValidatorInterface = typeof(IValidator<,>);
            var genericValidator = typeof(GenericValidator<>);
            var genericTranslateableValidator = typeof(GenericValidator<,>);
            var genericTableConsumingValidator = typeof(GenericTableConsumingValidator<,>);
            var genericTableConsumingTranslateableValidator = typeof(GenericTableConsumingTranslateableValidator<,,>);

            foreach (var type in typesToAdd) {
                Type concreteType;
                Type interfaceType;
                if (type.IsSubclassOfRawGeneric(tableConsumingTranslateableEntityType)) {
                    Type translationType = null;
                    var tempType = type.BaseType;

                    while (tempType?.BaseType != null && tempType.GenericTypeArguments.Length < 2) {
                        tempType = tempType.BaseType;
                    }

                    while (tempType != null) {
                        translationType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (translationType != null) { break; }
                        tempType = tempType.BaseType;
                    }

                    tempType = type.BaseType;
                    while (tempType?.BaseType != null && tempType.GenericTypeArguments.Length < 2) {
                        tempType = tempType.BaseType;
                    }

                    Type definingType = null;
                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments[1];
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }

                    concreteType = genericTableConsumingTranslateableValidator.MakeGenericType(type, translationType, definingType);
                    interfaceType = tableConsumingValidatorInterface.MakeGenericType(type, definingType);
                } else if (type.IsSubclassOfRawGeneric(translateableEntityType)) {
                    Type translationType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        translationType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (translationType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTranslateableValidator.MakeGenericType(type, translationType);
                    interfaceType = validatorInterface.MakeGenericType(type);
                } else if (type.IsSubclassOfRawGeneric(tableConsumingTranslationEntityType)) {
                    Type definingType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableConsumingValidator.MakeGenericType(type, definingType);
                    interfaceType = tableConsumingValidatorInterface.MakeGenericType(type, definingType);
                } else if (type.IsSubclassOfRawGeneric(tableConsumingEntityType)) {
                    Type definingType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableConsumingValidator.MakeGenericType(type, definingType);
                    interfaceType = tableConsumingValidatorInterface.MakeGenericType(type, definingType);
                } else {
                    concreteType = genericValidator.MakeGenericType(type);
                    interfaceType = validatorInterface.MakeGenericType(type);
                }
                builder.RegisterType(concreteType)
                       .As(interfaceType)
                       .FindConstructorsWith(InternalConstructorFinder);
            }
        }

        private void LoadGenericRepositories(ContainerBuilder builder, List<Type> types) {
            var translationEntityType = typeof(TranslationEntity);
            var translateableEntityType = typeof(TranslateableEntity<>);
            var tableConsumingTranslationEntityType = typeof(TableConsumingTranslationEntity<>);
            var tableConsumingTranslateableEntityType = typeof(TableConsumingTranslateableEntity<,>);

            var repositoryType = typeof(Repository<>);
            var concreteImplementedTypes = LoadableAssemblyTypes.Where(t => !t.IsGenericType && (t.IsSubclassOfRawGeneric(repositoryType)))
                                                                .Select(t => t.BaseType?.GenericTypeArguments.FirstOrDefault())
                                                                .Where(t => t != null).ToSafeHashSet();
            var typesToAdd = types.Where(t => !concreteImplementedTypes.Contains(t)).ToList();

            var repositoryInterface = typeof(IRepository<>);
            var translationEntityRepositoryInterface = typeof(ITranslationEntityRepository<>);
            var tableConsumingTranslationEntityRepositoryInterface = typeof(ITableConsumingTranslationEntityRepository<,>);
            var genericTranslationEntityRepositoryType = typeof(GenericTranslationEntityRepository<>);
            var genericTranslateableRepositoryType = typeof(GenericTranslateableRepository<,>);
            var genericTableConsumingTranslationEntityRepositoryType = typeof(GenericTableConsumingTranslationEntityRepository<,>);
            var genericTableConsumingTranslateableRepositoryType = typeof(GenericTableConsumingTranslateableRepository<,,>);
            var genericRepository = typeof(GenericRepository<>);

            foreach (var type in typesToAdd) {
                Type concreteType;
                Type interfaceType;
                if (type.IsSubclassOfRawGeneric(tableConsumingTranslateableEntityType)) {
                    Type translationType = null;
                    Type definingType = null;
                    var tempType = type.BaseType;
                    while (tempType?.BaseType != null && tempType.GenericTypeArguments.Length < 2) {
                        tempType = tempType.BaseType;
                    }

                    while (tempType != null) {
                        translationType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (translationType != null) {
                            definingType = tempType.GenericTypeArguments[1];
                            break;
                        }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableConsumingTranslateableRepositoryType.MakeGenericType(type, translationType, definingType);
                    interfaceType = repositoryInterface.MakeGenericType(type);
                } else if (type.IsSubclassOfRawGeneric(tableConsumingTranslationEntityType)) {
                    Type definingType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        definingType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (definingType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTableConsumingTranslationEntityRepositoryType.MakeGenericType(type, definingType);
                    interfaceType = repositoryInterface.MakeGenericType(type);
                    builder.RegisterType(genericTableConsumingTranslationEntityRepositoryType.MakeGenericType(type, definingType))
                           .As(tableConsumingTranslationEntityRepositoryInterface.MakeGenericType(type, definingType))
                           .FindConstructorsWith(InternalConstructorFinder);
                } else if (type.IsSubclassOfRawGeneric(translateableEntityType)) {
                    Type translationType = null;
                    var tempType = type.BaseType;
                    while (tempType != null) {
                        translationType = tempType.GenericTypeArguments.FirstOrDefault();
                        if (translationType != null) { break; }
                        tempType = tempType.BaseType;
                    }
                    concreteType = genericTranslateableRepositoryType.MakeGenericType(type, translationType);
                    interfaceType = repositoryInterface.MakeGenericType(type);
                } else if (translationEntityType.IsAssignableFrom(type)) {
                    concreteType = genericTranslationEntityRepositoryType.MakeGenericType(type);
                    interfaceType = repositoryInterface.MakeGenericType(type);
                    builder.RegisterType(genericTranslationEntityRepositoryType.MakeGenericType(type))
                           .As(translationEntityRepositoryInterface.MakeGenericType(type))
                           .FindConstructorsWith(InternalConstructorFinder);
                } else {
                    concreteType = genericRepository.MakeGenericType(type);
                    interfaceType = repositoryInterface.MakeGenericType(type);
                }
                builder.RegisterType(concreteType)
                       .As(interfaceType)
                       .FindConstructorsWith(InternalConstructorFinder);
            }
        }

        private void LoadServices(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IBaseService))
                   .AsImplementedInterfaces();
        }

        private void LoadThirdPartyFactories(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IThirdPartyFactory))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
        }

        private void LoadValidators(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IBaseValidator))
                   .AsImplementedInterfaces();
        }

        private void LoadRepositories(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IBaseRepository))
                   .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IPrimitiveRepository))
                   .AsImplementedInterfaces();
            builder.RegisterAssemblyOpenGenericTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .As(typeof(IDatabaseProvider<>));
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AsClosedTypesOf(typeof(IBaseMigrationInformationProvider<>));
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AssignableTo(typeof(IAdvisoryLockProvider))
                   .AsImplementedInterfaces();
            BindRepository(builder, typeof(IRepository<>), LoadableAssemblies);
            BindRepository(builder, typeof(IBaseResultRepository<>), LoadableAssemblies);
            BindRepository(builder, typeof(IReadonlyRepository<>), LoadableAssemblies);
            builder.RegisterType<OfflineTableExistsRepository>()
                   .As<IOfflineTableExistsRepository>()
                   .FindConstructorsWith(InternalConstructorFinder)
                   .WithParameter("checkTableExistence", BuildOptions.IsOfflineBuild);
        }

        private void LoadMigrations(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AsClosedTypesOf(typeof(IMigration<>));

            builder.RegisterGeneric(typeof(DatabaseMigratorFactory<>))
                   .FindConstructorsWith(InternalConstructorFinder)
                   .As(typeof(IDatabaseMigratorFactory<>));

            builder.RegisterGeneric(typeof(MigrationFinder<>))
                   .FindConstructorsWith(InternalConstructorFinder)
                   .As(typeof(IMigrationFinder<>));
        }

        private void LoadOptimizations(ContainerBuilder builder) {
            builder.RegisterAssemblyTypes(LoadableAssemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AsClosedTypesOf(typeof(IOptimization<>));

            builder.RegisterGeneric(typeof(DatabaseOptimizerFactory<>))
                   .FindConstructorsWith(InternalConstructorFinder)
                   .As(typeof(IDatabaseOptimizerFactory<>));
        }

        private static void BindRepository(ContainerBuilder builder, Type t, Assembly[] assemblies) {
            builder.RegisterAssemblyTypes(assemblies)
                   .FindConstructorsWith(InternalConstructorFinder)
                   .AsClosedTypesOf(t);
        }

        private static void LoadGeneral(ContainerBuilder builder) {
            builder.RegisterType<MemoryCacheWrapper>().As<IMemoryCacheWrapper>();
            builder.RegisterGeneric(typeof(DatabaseMigrator<>)).As(typeof(IMigrator<>));
            builder.RegisterGeneric(typeof(DatabaseOptimizer<>)).As(typeof(IOptimizer<>));
        }
        
        public static void RegisterConnection(ContainerBuilder builder, string connection) {
            builder.RegisterType<SqlDbConnectionFactory>()
                   .As<IDbConnectionFactory>()
                   .FindConstructorsWith(InternalConstructorFinder)
                   .WithParameter("connection", connection);
        }

        /// <summary>
        /// Load all the connections, factories and contexts
        /// </summary>
        /// <param name="builder">The containerbuilder</param>
        private void LoadContextFactories(ContainerBuilder builder) {
            builder.RegisterType<DbContextFactory>().As<IDbContextFactory>()
                   .FindConstructorsWith(InternalConstructorFinder).InstancePerLifetimeScope();
            builder.RegisterType<RemoteDbConnectionFactory>().As<IRemoteDbConnectionFactory>()
                   .FindConstructorsWith(InternalConstructorFinder).InstancePerLifetimeScope();
            builder.RegisterType<RemoteDbContextFactory>().As<IRemoteDbContextFactory>()
                   .FindConstructorsWith(InternalConstructorFinder).InstancePerLifetimeScope();
            builder.RegisterType<PowershellConnectionFactory>().As<IPowershellConnectionFactory>()
                   .FindConstructorsWith(InternalConstructorFinder).InstancePerLifetimeScope();
            builder.RegisterType<PowershellContextFactory>().As<IPowershellContextFactory>()
                   .FindConstructorsWith(InternalConstructorFinder).InstancePerLifetimeScope();
        }

        protected void LoadFactories(ContainerBuilder builder) {
            var baseType = typeof(IKeyedFactoryService<>);
            var factoryInterfaces = new KeyValueList<Type, MethodInfo>();
            foreach (var type in LoadableAssemblyTypes) {
                if (!type.IsInterface) { continue; }
                var interfaces = type.GetInterfaces();
                foreach (var @interface in interfaces) {
                    if (!@interface.IsGenericType) { continue; }
                    if (@interface.GetGenericTypeDefinition() == baseType) {
                        factoryInterfaces.Add(type, @interface.GetProperty("Type").GetMethod);
                    }
                }
            }
            var genericFactoryServiceType = typeof(FactoryService<,>);
            var genericFactoryServiceInterfaceType = typeof(IFactoryService<,>);
            var genericFuncType = typeof(Func<,>);
            foreach (var factoryInterface in factoryInterfaces) {
                var implementations = LoadableAssemblyTypes.Where(t => factoryInterface.Key.IsAssignableFrom(t) && !t.IsAbstract)
                                                           .ToList();
                foreach (var implementation in implementations) {
                    var instance = TypeActivator.CreateInstance(implementation);
                    var actualType = factoryInterface.Value.Invoke(instance, null);
                    builder.RegisterType(implementation)
                           .FindConstructorsWith(InternalConstructorFinder)
                           .As(factoryInterface.Key)
                           .Keyed(actualType, factoryInterface.Key);
                }
                var implementedFactoryServiceType = genericFactoryServiceType.MakeGenericType(factoryInterface.Value.ReturnType, factoryInterface.Key);
                var implementedFactoryServiceInterfaceType = genericFactoryServiceInterfaceType.MakeGenericType(factoryInterface.Value.ReturnType, factoryInterface.Key);
                var implementedGenericFuncType = genericFuncType.MakeGenericType(factoryInterface.Value.ReturnType, factoryInterface.Key);
                builder.RegisterType(implementedFactoryServiceType)
                       .FindConstructorsWith(InternalConstructorFinder)
                       .As(implementedFactoryServiceInterfaceType);
            }
        }

        [Obsolete("Is automatically done by " + nameof(LoadFactories))]
        protected void LoadFactory<T, TKey>(ContainerBuilder builder) where T : class, IKeyedFactoryService<TKey> where TKey : Enum {
            var interfaceType = typeof(T);
            var implementations = LoadableAssemblyTypes.Where(t => interfaceType.IsAssignableFrom(t) && !t.IsAbstract)
                                                       .ToList();
            foreach (var implementation in implementations) {
                var instance = (T)TypeActivator.CreateInstance(implementation);
                builder.RegisterType(implementation)
                       .FindConstructorsWith(InternalConstructorFinder)
                       .As(interfaceType)
                       .Keyed(instance.Type, interfaceType);
            }
            builder.RegisterType<FactoryService<TKey, T>>()
                   .FindConstructorsWith(InternalConstructorFinder)
                   .As<IFactoryService<TKey, T>>();
            builder.Register<Func<TKey, T>>(c => {
                var componentContext = c.Resolve<IComponentContext>();
                return serviceType => componentContext.ResolveKeyed<T>(serviceType);
            });
        }

        protected static void BindTo<T, TImplementation>(ContainerBuilder builder) where TImplementation : T {
            builder.RegisterType<TImplementation>().As<T>()
                   .FindConstructorsWith(InternalConstructorFinder);
        }

        public static ConstructorFinder InternalConstructorFinder = new ConstructorFinder(BindingFlags.Default | BindingFlags.NonPublic
                                                                                                               | BindingFlags.Public | BindingFlags.Instance);


        public class ConstructorFinder : IConstructorFinder {
            private readonly BindingFlags _bindingFlags;

            public ConstructorFinder(BindingFlags flags) {
                _bindingFlags = flags;
            }

            public ConstructorInfo[] FindConstructors(Type targetType) {
                return targetType.GetConstructors(_bindingFlags);
            }
        }

        protected static void Warmup<T, TInterface>(ILifetimeScope container) where T : struct, Enum {
            foreach (var value in EnumHelper.GetValues<T>()) {
                if (!container.IsRegisteredWithKey<TInterface>(value)) {
                    continue;
                }
                container.ResolveKeyed<TInterface>(value);
            }
        }

        private void InjectIsCacheEnabledProperty(object sender, object instance) {
            if (!_hasCachableTypes) { return; }
            var cReg = sender as ComponentRegistration;
            var type = cReg?.Activator.LimitType;
            var instanceType = type ?? instance.GetType();
            if (!typeof(ICacheable).IsAssignableFrom(instanceType)) { return; }
            var cacheInfo = _cacheableTypes.TryGetValueWithDefault(instanceType);
            if (cacheInfo == null) {
                var temp = _cacheableTypes.FirstOrDefault(t => t.Key.IsAssignableFrom(instanceType));
                cacheInfo = temp.Value;
                if (temp.Key != null) {
                    SetCache(instanceType, cacheInfo.Item2);
                    cacheInfo = _cacheableTypes.TryGetValueWithDefault(instanceType);
                }
            }
            if (cacheInfo == null) {
                var addResult = _cacheableTypes.TryAdd(instanceType, Tuple.Create((PropertyInfo)null, false));
                if (addResult) {
                    _hasCachableTypes = true;
                }
                return;
            }
            if (!cacheInfo.Item2) { return; }
            var cacheProperty = cacheInfo.Item1;
            if (cacheProperty != null) {
                var isCachingEnabled = cacheInfo.Item2;
                cacheProperty.SetValue(instance, isCachingEnabled);
            }
        }

        protected void SetCache<T>(bool enabled) {
            SetCache(typeof(T), enabled);
        }

        protected void SetCache(Type type, bool enabled) {
            if (type == null) { return; }
            var cacheProperty = type.GetProperty("IsCachingEnabled", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var addResult = _cacheableTypes.TryAdd(type, Tuple.Create(cacheProperty, enabled));
            if (addResult) {
                _hasCachableTypes = true;
            }
        }

        protected List<Type> GetLoadableTypesForAssembly(Assembly assembly) {
            if (!_assemblyLoadableTypes.TryGetValue(assembly, out var loadableTypes)) {
                loadableTypes = assembly.GetAllLoadableTypes().ToList();
                _assemblyLoadableTypes.Add(assembly, loadableTypes);
            }
            return loadableTypes;
        }
    }
}