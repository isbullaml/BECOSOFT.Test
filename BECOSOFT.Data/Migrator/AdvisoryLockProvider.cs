using BECOSOFT.Data.Context;
using System;
using System.Data;

namespace BECOSOFT.Data.Migrator {
    internal abstract class AdvisoryLockProvider : IAdvisoryLockProvider {
        private readonly IDbConnectionFactory _connectionFactory;
        private IDbConnection _connection;

        protected IDbConnection Connection {
            get {
                SetConnection();
                return _connection;
            }
        }

        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(600);
        protected string AdvisoryLockName => "BECOSOFT.Data.AdvisoryLockedProvider";
        protected int MaxDescriptionLength { get; set; }

        protected AdvisoryLockProvider(IDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        private void SetConnection() {
            if (_connection == null) {
                _connection = _connectionFactory.CreateConnection();
            }
            if (_connection.State != ConnectionState.Open) {
                _connection.Open();
            }
        }

        public abstract IDbConnection Begin();
        public abstract void End();
        public abstract void AcquireAdvisoryLock();
        public abstract void ReleaseAdvisoryLock();
    }
}