using BECOSOFT.Utilities.Attributes;
using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Utilities.CommandLine {
    /// <summary>
    /// Argument container.
    /// Parses commandline args and fills the implementing argument container properties (decorated with <see cref="CommandLineArgumentAttribute"/>).
    /// <para>Arguments should start with -- when calling the executable</para>
    /// </summary>
    public abstract class BaseArgumentContainer {
        public IReadOnlyDictionary<string, string> Arguments { get; set; }

        /// <summary>
        /// Load the argument container for the given args.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T Load<T>(string[] args) where T : BaseArgumentContainer, new() {
            var container = new T();
            var argumentMapping = GetArgumentMapping<T>();

            var parsedArgs = new Dictionary<string, string>();
            var index = 0;
            do {
                var arg = args[index];
                var nextArg = args[index + 1];
                try {
                    if (!arg.StartsWith("--")) { continue; }
                    var parsedKey = arg.Substring(2).ToLowerInvariant();
                    parsedArgs.Add(parsedKey, nextArg);

                    if (!argumentMapping.TryGetValue(parsedKey, out var propertyInformation)) { continue; }
                    var parsedValue = Converter.GetDelegate(propertyInformation.Item2)(nextArg);
                    propertyInformation.Item1.Invoke(container, new[] { parsedValue });
                } finally {
                    index++;
                }
            } while (index < args.Length - 1);

            container.Arguments = parsedArgs;
            return container;
        }

        /// <summary>
        /// Load arguments from <see cref="Environment"/>.<see cref="Environment.GetCommandLineArgs"/>.
        /// <para>The first argument is skipped, since it contains the full executable path</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Load<T>() where T : BaseArgumentContainer, new() {
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            return Load<T>(args);
        }

        private static Dictionary<string, Tuple<MethodInfo, Type>> GetArgumentMapping<T>() where T : BaseArgumentContainer, new() {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Select(p => Tuple.Create(p, p.GetCustomAttribute<CommandLineArgumentAttribute>()))
                                      .Where(p => p.Item2 != null && p.Item1.CanWrite)
                                      .ToList();

            var argumentMapping = properties.ToDictionary(p => p.Item2.ArgumentName.ToLowerInvariant().Replace("--", ""), p => Tuple.Create(p.Item1.SetMethod, p.Item1.PropertyType));
            return argumentMapping;
        }
    }
}