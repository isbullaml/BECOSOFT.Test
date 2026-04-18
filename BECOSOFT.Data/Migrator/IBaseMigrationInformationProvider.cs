using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Migrator {
    public interface IBaseMigrationInformationProvider<T> where T : Enum {
        string TableName { get; }
        string TableNameDynamic { get; }
        string SchemaName { get; }
        Type DescriptionAttributeType { get; }
        List<Type> MigrationAttributes { get; }
        Dictionary<T, List<T>> LinkedMigrationTypes { get; }
        
        bool ShouldRegisterVersion(T migrationType);
        Type GetMigrationClassType(T migrationType);
        bool IsDynamic(T migrationType);
    }
}