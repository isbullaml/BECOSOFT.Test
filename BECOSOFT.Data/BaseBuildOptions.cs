using System;
using System.Collections.Generic;
using Autofac;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Utilities.Exceptions;
using System.Data.SqlClient;

namespace BECOSOFT.Data {
    public class BaseBuildOptions {
        public bool NoConnection { get; }
        /// <summary>
        /// The <see cref="Connection"/> to use for this Kernel. Only one <see cref="Connection"/> can be set per Kernel.
        /// </summary>
        public string Connection { get; }

        /// <summary>
        /// If set, this action is performed on the <see cref="ContainerBuilder"/> after all default registrations
        /// </summary>
        public Action<ContainerBuilder> BuilderAction { get; set; }

        /// <summary>
        /// List of the names of assemblies to load. (Without the .dll-extension)
        /// </summary>
        public List<string> AssembliesToLoad { get; set; }

        /// <summary>
        /// Perform a connection test (<see cref="SqlConnection"/>.<see cref="SqlConnection.Open"/>)
        /// </summary>
        public bool CheckConnection { get; set; }

        /// <summary>
        /// Indicates that the resulting <see cref="IContainer"/> is used on an offline database, which might not have all tables present.
        /// </summary>
        public bool IsOfflineBuild { get; set; }

        /// <summary>
        /// Initializes the <see cref="BaseBuildOptions"/> object with the provided constructor elements.
        /// </summary>
        /// <param name="connection">The <see cref="Connection"/> to use for this Kernel. Only one <see cref="Connection"/> can be set per Kernel.</param>
        /// <exception cref="DbConnectionException">Throws an exception when the <see cref="BaseBuildOptions.Connection"/> (<see cref="BaseBuildOptions"/>) is not a valid connection string</exception>
        public BaseBuildOptions(string connection) {
            if (!SqlConnectionHelper.IsValid(connection)) {
                throw new DbConnectionException(Resources.Error_Connection_Invalid);
            }

            Connection = connection;
            AssembliesToLoad = new List<string>(0);
            NoConnection = false;
        }

        /// <summary>
        /// Initializes the <see cref="BaseBuildOptions"/> object without connection
        /// </summary>
        public BaseBuildOptions() {
            AssembliesToLoad = new List<string>(0);
            NoConnection = true;
        }
    }
}