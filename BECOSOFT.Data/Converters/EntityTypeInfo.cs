using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class containing all info about an entity
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class EntityTypeInfo {
        /// <summary>
        /// The type of the entity
        /// </summary>
        public Type EntityType { get; }
        /// <summary>
        /// Value indicating whether the entity is a base entity
        /// </summary>
        public bool IsBaseEntity { get; private set; }
        /// <summary>
        /// Value indicating whether the entity is a base result
        /// </summary>
        public bool IsBaseResult { get; private set; }

        /// <summary>
        /// Value indicating whether the property is a filter
        /// </summary>
        public bool IsFilter { get; set; }
        /// <summary>
        /// The table-info
        /// </summary>
        public TableDefinition TableDefinition { get; set; }
        /// <summary>
        /// Value indicating whether the entity is table consuming
        /// </summary>
        public bool IsTableConsuming { get; private set; }

        private TableAttribute _tableAttribute;

        /// <summary>
        /// The info about all the properties
        /// </summary>
        public IList<EntityPropertyInfo> Properties { get; }
        /// <summary>
        /// Dictionary containing all info about the properties by name
        /// </summary>
        private Dictionary<string, EntityPropertyInfo> PropertiesByPropertyName { get; }

        /// <summary>
        /// List containing all insertable properties
        /// </summary>
        public List<EntityPropertyInfo> InsertableProperties { get; private set; }

        /// <summary>
        /// List containing all updateable properties
        /// </summary>
        public List<EntityPropertyInfo> UpdateableProperties { get; private set; }

        /// <summary>
        /// List containing all linked entity properties
        /// </summary>
        public List<EntityPropertyInfo> LinkedEntityProperties { get; private set; }
        /// <summary>
        /// List containing all inverse linked entity properties
        /// </summary>
        public List<EntityPropertyInfo> InverseLinkedEntityProperties { get; private set; }
        /// <summary>
        /// List containing all inverse linked base child properties
        /// </summary>
        public List<EntityPropertyInfo> InverseLinkedBaseChildProperties { get; } = new List<EntityPropertyInfo>();
        /// <summary>
        /// List containing all linked entities properties
        /// </summary>
        public List<EntityPropertyInfo> LinkedEntitiesProperties { get; private set; }
        /// <summary>
        /// List containing all linked basechild properties
        /// </summary>
        public List<EntityPropertyInfo> LinkedBaseChildProperties { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<EntityPropertyInfo, EntityPropertyInfo> UpdateableIncludingBaseChildProperties { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<EntityPropertyInfo, EntityPropertyInfo> InsertableIncludingBaseChildProperties { get; private set; }

        /// <summary>
        /// List containing all linked base result properties
        /// </summary>
        public List<EntityPropertyInfo> LinkedBaseResultProperties { get; private set; }
        /// <summary>
        /// List containing all linked base results properties
        /// </summary>
        public List<EntityPropertyInfo> LinkedBaseResultsProperties { get; private set; }

        /// <summary>
        /// List containing all properties to use in a SELECT
        /// </summary>
        public List<EntityPropertyInfo> SelectProperties { get; private set; }

        /// <summary>
        /// List containing all properties to use in the validator
        /// </summary>
        public List<EntityPropertyInfo> ValidatableProperties { get; private set; }
        /// <summary>
        /// List containing all properties to use in a search
        /// </summary>
        public List<EntityPropertyInfo> SearchProperties { get; private set; }

        /// <summary>
        /// Info about the primary key property
        /// </summary>
        public EntityPropertyInfo PrimaryKeyInfo { get; private set; }

        public string TablePrimaryKey { get; set; }
        public EntityPropertyInfo TablePrimaryKeyInfo { get; private set; }

        public bool HasLinkedEntityProperties { get; private set; }
        public bool HasLinkedBaseResultProperties { get; private set; }

        internal EntityTypeInfo(Type type) {
            EntityType = type;
            Properties = new List<EntityPropertyInfo>();
            ValidatableProperties = new List<EntityPropertyInfo>();
            PropertiesByPropertyName = new Dictionary<string, EntityPropertyInfo>();

            var primaryKeyColumnName = GetTableInfo(type);
            TablePrimaryKey = primaryKeyColumnName;

            GetTypeInfo(type);
            GetPropertyInfo(type, primaryKeyColumnName);

            SetLists();
        }

        public EntityPropertyInfo GetPropertyInfo(string propertyName, string tablePart) {
            if (propertyName.IsNullOrWhiteSpace()) {
                return null;
            }
            var lowerProp = propertyName.ToLowerInvariant();
            var lowerTablePart = tablePart?.ToLowerInvariant();
            if (PropertiesByPropertyName.TryGetValue(lowerProp, out var propInfo)) {
                return propInfo;
            }
            foreach (var propertyInfo in Properties) {
                string propName;
                string propColName;
                if (propertyInfo.HasFormatSpecifier) {
                    propName = propertyInfo.PropertyName.FormatWith(lowerTablePart);
                    propColName = propertyInfo.LowerCaseColumnName.FormatWith(lowerTablePart);
                } else {
                    propName = propertyInfo.PropertyName;
                    propColName = propertyInfo.LowerCaseColumnName;
                }
                if (propName.EqualsIgnoreCase(propertyName)
                    || propColName.Equals(lowerProp)) {
                    return propertyInfo;
                }
            }
            return null;
        }

        private void SetLists() {
            InsertableProperties = Properties.Where(p => p.Insertable).ToList();
            UpdateableProperties = Properties.Where(p => p.Updateable).ToList();
            UpdateableIncludingBaseChildProperties = new Dictionary<EntityPropertyInfo, EntityPropertyInfo>();
            InsertableIncludingBaseChildProperties = new Dictionary<EntityPropertyInfo, EntityPropertyInfo>();
            foreach (var property in Properties) {
                if (property.IsBaseChild) {
                    var entityTypeInfo = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                    foreach (var childProperty in entityTypeInfo.Properties) {
                        if (childProperty.Insertable) {
                            InsertableIncludingBaseChildProperties.Add(childProperty, property);
                        }
                        if (childProperty.Updateable) {
                            UpdateableIncludingBaseChildProperties.Add(childProperty, property);
                        }
                    }
                } else {
                    if (property.Insertable) {
                        InsertableIncludingBaseChildProperties.Add(property, null);
                    }

                    if (property.Updateable) {
                        UpdateableIncludingBaseChildProperties.Add(property, null);
                    }
                }
            }
            LinkedEntityProperties = Properties.Where(p => p.IsLinkedEntity).ToList();
            InverseLinkedEntityProperties = Properties.Where(p => p.IsInverseLinkedEntity).ToList();
            LinkedEntitiesProperties = Properties.Where(p => p.AreLinkedEntities).ToList();
            LinkedBaseChildProperties = Properties.Where(p => p.IsBaseChild).ToList();
            LinkedBaseResultProperties = Properties.Where(p => p.IsLinkedBaseResult).ToList();
            LinkedBaseResultsProperties = Properties.Where(p => p.AreLinkedBaseResults).ToList();
            SelectProperties = Properties.Where(p => !p.IsLinked).ToList();
            SearchProperties = Properties.Where(p => p.IsSearchable).ToList();
            HasLinkedEntityProperties = LinkedEntityProperties.HasAny() || LinkedEntitiesProperties.HasAny() || InverseLinkedEntityProperties.HasAny();
            HasLinkedBaseResultProperties = LinkedBaseResultProperties.HasAny() || LinkedBaseResultsProperties.HasAny() || InverseLinkedEntityProperties.HasAny();
        }

        private string GetTableInfo(Type type) {
            _tableAttribute = type.GetAttribute<TableAttribute>(true);
            if (_tableAttribute == null) {
                return null;
            }

            TableDefinition = _tableAttribute.ToDefinition();
            return _tableAttribute.PrimaryKeyColumn;
        }

        private void GetTypeInfo(Type type) {
            IsBaseResult = type.IsSubclassOf(typeof(BaseResult));
            IsBaseEntity = type.IsSubclassOf(typeof(BaseEntity)) || type.IsSubclassOf(typeof(BaseChild));
            IsTableConsuming = type.IsSubclassOfRawGeneric(typeof(TableConsumingEntity<>)) || type.IsSubclassOfRawGeneric(typeof(TableConsumingResult<>));
            IsFilter = type.IsSubclassOf(typeof(Filter));
        }

        private void GetPropertyInfo(Type type, string primaryKeyColumnName) {
            var typeProperties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(p => p.CanWrite).OrderBy(x => x.MetadataToken).ToList();
            var hasNonSearchableProperties = IsBaseResult && typeProperties.Any(p => p.GetCustomAttribute<NotSearchableAttribute>() != null);
            foreach (var property in typeProperties) {
                if (IsBaseResult) {
                    GetBaseResultPropertyInfo(property, hasNonSearchableProperties);
                } else {
                    GetBaseEntityPropertyInfo(property, primaryKeyColumnName);
                }
            }
        }

        private void GetBaseResultPropertyInfo(PropertyInfo property, bool hasNonSearchableProperties) {
            var colName = "";
            string foreignKeyColumn = null;
            string ownKeyColumn = null;
            FieldType fieldType = 0;
            var isInverseLinkedEntity = false;
            var isBaseChild = false;
            var isLinkedBaseResult = false;
            var areLinkedBaseResults = false;
            var onlyAddToValidatableProperties = false;
            var columnAttribute = property.GetAttribute<ColumnAttribute>(true);
            var isSearchable = hasNonSearchableProperties && property.GetAttribute<NotSearchableAttribute>(true) == null;

            if (property.PropertyType.IsSubclassOf(typeof(BaseChild))) {
                var linkedBaseChildAttribute = property.GetAttribute<LinkedBaseChildAttribute>(true);
                if (linkedBaseChildAttribute == null) {
                    return;
                }
                isBaseChild = true;
                var baseChildEntityType = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                InverseLinkedBaseChildProperties.AddRange(baseChildEntityType.InverseLinkedEntityProperties);
            } else if (property.PropertyType.IsInterfaceImplementationOf<IEntity>()) {
                var linkedBaseResultAttribute = property.GetAttribute<LinkedBaseResultAttribute>(true);
                var inverseLinkedEntityAttribute = property.GetAttribute<InverseLinkedEntityAttribute>(true);
                if (inverseLinkedEntityAttribute != null) {
                    isInverseLinkedEntity = true;
                    foreignKeyColumn = inverseLinkedEntityAttribute.ForeignKeyProperty;
                } else if (linkedBaseResultAttribute != null) {
                    isLinkedBaseResult = true;
                    foreignKeyColumn = linkedBaseResultAttribute.ForeignKeyColumn;
                    ownKeyColumn = linkedBaseResultAttribute.OwnKeyColumn;
                } else {
                    return;
                }
            } else if (IsListOfIEntity(property)) {
                var linkedBaseResultsAttribute = property.GetAttribute<LinkedBaseResultsAttribute>(true);
                if (linkedBaseResultsAttribute != null) {
                    areLinkedBaseResults = true;
                    foreignKeyColumn = linkedBaseResultsAttribute.ForeignKeyColumn;
                    ownKeyColumn = linkedBaseResultsAttribute.OwnKeyColumn;
                } else {
                    return;
                }
            } else if (columnAttribute != null) {
                colName = columnAttribute.Column;
                fieldType = GetFieldType(columnAttribute.FieldType, property.PropertyType);
            } else if (property.GetSetMethod(true) != null) {
                onlyAddToValidatableProperties = true;
            } else {
                return;
            }

            var entityPropertyInfo = new EntityPropertyInfo(this, property.Name, colName.Trim(), false) {
                PropertyInfo = property,
                DatabaseType = fieldType,
                IsBaseChild = isBaseChild,
                IsInverseLinkedEntity = isInverseLinkedEntity,
                IsLinkedBaseResult = isLinkedBaseResult,
                AreLinkedBaseResults = areLinkedBaseResults,
                ForeignKeyColumn = foreignKeyColumn,
                PrimaryKeyColumn = ownKeyColumn,
                IsAbstract = property.GetSetMethod()?.IsAbstract ?? false,
                IsSearchable = isSearchable,
            };

            SetSettersAndGetters(entityPropertyInfo, property, !onlyAddToValidatableProperties);

            ValidatableProperties.Add(entityPropertyInfo);
            if (onlyAddToValidatableProperties) {
                return;
            }

            Properties.Add(entityPropertyInfo);
            PropertiesByPropertyName.Add(property.Name.ToLowerInvariant(), entityPropertyInfo);
            if (!TablePrimaryKey.IsNullOrWhiteSpace() && (property.Name.EqualsIgnoreCase(TablePrimaryKey) || colName.EqualsIgnoreCase(TablePrimaryKey))) {
                TablePrimaryKeyInfo = entityPropertyInfo;
            }
        }

        private void GetBaseEntityPropertyInfo(PropertyInfo property, string primaryKeyColumnName) {
            var colName = "";
            FieldType fieldType = 0;
            var isInverseLinkedEntity = false;
            var isLinkedEntity = false;
            var isBaseChild = false;
            var areLinkedEntities = false;
            var insertable = false;
            var updateable = false;
            var isPrimaryKey = false;
            var isFilter = false;
            var onlyAddToValidatableProperties = false;
            var isSearchable = property.GetAttribute<NotSearchableAttribute>(true) == null;
            string foreignKeyColumn = null;
            if (property.Name.Equals("ID", StringComparison.InvariantCultureIgnoreCase) && primaryKeyColumnName != null) {
                colName = primaryKeyColumnName;
                fieldType = FieldType.Numeric;
                isPrimaryKey = true;
            } else if (property.PropertyType.IsSubclassOf(typeof(BaseChild))) {
                var linkedBaseChildAttribute = property.GetAttribute<LinkedBaseChildAttribute>(true);
                if (linkedBaseChildAttribute == null) {
                    return;
                }
                isBaseChild = true;
                insertable = true;
                updateable = true;
                var baseChildEntityType = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                InverseLinkedBaseChildProperties.AddRange(baseChildEntityType.InverseLinkedEntityProperties);
            } else if (property.PropertyType.IsInterfaceImplementationOf<IEntity>()) {
                var inverseLinkedEntityAttribute = property.GetAttribute<InverseLinkedEntityAttribute>(true);
                if (inverseLinkedEntityAttribute != null) {
                    isInverseLinkedEntity = true;
                    foreignKeyColumn = inverseLinkedEntityAttribute.ForeignKeyProperty;
                } else {
                    var linkedEntityAttribute = property.GetAttribute<LinkedEntityAttribute>(true);
                    if (linkedEntityAttribute != null) {
                        isLinkedEntity = true;
                        foreignKeyColumn = linkedEntityAttribute.ForeignKeyColumn;
                    } else {
                        return;
                    }
                }
            } else if (IsListOfIEntity(property)) {
                var linkedEntitiesAttribute = property.GetAttribute<LinkedEntitiesAttribute>(true);
                if (linkedEntitiesAttribute != null) {
                    areLinkedEntities = true;
                    foreignKeyColumn = linkedEntitiesAttribute.ForeignKeyColumn;
                } else {
                    return;
                }
            } else if (property.PropertyType.IsSubclassOf(typeof(Filter))) {
                isFilter = true;
            } else {
                var columnAttribute = property.GetAttribute<ColumnAttribute>(true);
                if (columnAttribute == null) {
                    if (property.GetSetMethod(true) != null) {
                        onlyAddToValidatableProperties = true;
                    } else {
                        return;
                    }
                } else {
                    colName = columnAttribute.Column;
                    fieldType = GetFieldType(columnAttribute.FieldType, property.PropertyType);
                    insertable = columnAttribute.Insertable;
                    updateable = columnAttribute.Updateable;
                }
            }
            var entityPropertyInfo = new EntityPropertyInfo(this, property.Name, colName.Trim(), isPrimaryKey) {
                PropertyInfo = property,
                DatabaseType = fieldType,
                IsLinkedEntity = isLinkedEntity,
                IsInverseLinkedEntity = isInverseLinkedEntity,
                AreLinkedEntities = areLinkedEntities,
                Insertable = insertable,
                Updateable = updateable,
                ForeignKeyColumn = foreignKeyColumn,
                IsBaseChild = isBaseChild,
                IsSearchable = isSearchable,
                IsAbstract = property.GetSetMethod()?.IsAbstract ?? false,
                IsFilter = isFilter
            };
            SetSettersAndGetters(entityPropertyInfo, property, !onlyAddToValidatableProperties);

            ValidatableProperties.Add(entityPropertyInfo);
            if (onlyAddToValidatableProperties) {
                return;
            }

            Properties.Add(entityPropertyInfo);
            PropertiesByPropertyName.Add(property.Name.ToLowerInvariant(), entityPropertyInfo);
            if (isPrimaryKey) {
                PrimaryKeyInfo = entityPropertyInfo;
            }
        }

        private static bool IsListOfIEntity(PropertyInfo property) {
            var genericListParameter = property.PropertyType.GetGenericArguments().FirstOrDefault();
            return genericListParameter != null && property.PropertyType.IsGenericList() && genericListParameter.IsInterfaceImplementationOf<IEntity>();
        }

        private static void SetSettersAndGetters(EntityPropertyInfo entityPropertyInfo, PropertyInfo property, bool includeSet) {
            var fastProperty = new FastProperty(property, includeSet: includeSet);
            entityPropertyInfo.Getter = fastProperty.GetDelegate;
            if (includeSet) {
                entityPropertyInfo.Setter = fastProperty.SetDelegate;
            }
        }

        private static FieldType GetFieldType(FieldType fieldType, Type type) {
            if (fieldType != 0) {
                return fieldType;
            }

            var typeToUse = type.GetNonNullableType();
            if (typeToUse == typeof(string)) {
                fieldType = FieldType.Char;
            } else if (typeToUse == typeof(DateTime)) {
                fieldType = FieldType.Date;
            } else if (typeToUse == typeof(TimeSpan)) {
                fieldType = FieldType.Time;
            } else if (typeToUse == typeof(char)) {
                fieldType = FieldType.Char;
            } else if (typeToUse == typeof(bool)) {
                fieldType = FieldType.Bit;
            } else if (typeToUse == typeof(byte[])) {
                fieldType = FieldType.Byte;
            } else {
                fieldType = FieldType.Numeric;
            }
            return fieldType;
        }

        private string DebuggerDisplay => $"{EntityType.Name}, Table: '{TableDefinition.FullTableName}'";
    }
}