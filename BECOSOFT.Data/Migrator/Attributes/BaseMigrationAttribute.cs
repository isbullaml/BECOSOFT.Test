using BECOSOFT.Utilities.Converters;
using System;

namespace BECOSOFT.Data.Migrator.Attributes {
    public abstract class BaseMigrationAttribute : Attribute {
        protected abstract object MigrationTypeValue { get; }
        public Type Type => MigrationTypeValue.GetType();

        public int Version { get; }

        public string Information { get; }

        public T GetType<T>() where T : Enum {
            return MigrationTypeValue.To<T>();
        }

        protected BaseMigrationAttribute(int version, string information) {
            Version = version;
            Information = information;
        }
    }
}