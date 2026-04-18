using BECOSOFT.Data.Models;
using System;

namespace BECOSOFT.Data.Context.Remote {
    /// <summary>
    /// Interface for context for executing SQL queries on remote servers
    /// </summary>
    internal interface IRemoteDbContext : IDisposable {
        /// <summary>
        /// Event for progress
        /// </summary>
        event RemoteDbContext.ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="query">The query-object</param>
        void Query(ParametrizedQuery query);

        /// <summary>
        /// Execute a query on a remote server with progress
        /// </summary>
        /// <param name="query">The query-object</param>
        /// <param name="timeout">The timeout</param>
        void ProgressQuery(ParametrizedQuery query);
    }
}