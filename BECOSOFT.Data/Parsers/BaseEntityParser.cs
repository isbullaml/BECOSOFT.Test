using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using System.Collections.Generic;

namespace BECOSOFT.Data.Parsers {
    internal class BaseEntityParser<T> : BaseParser<T> where T : BaseEntity {
        public BaseEntityParser(IOfflineTableExistsRepository tableExistsRepository) : base(tableExistsRepository) {
        }

        protected override List<EntityPropertyInfo> GetInverseLinkedEntityProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.InverseLinkedEntityProperties;
        protected override List<EntityPropertyInfo> GetInverseLinkedBaseChildProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.InverseLinkedBaseChildProperties;
        protected override List<EntityPropertyInfo> GetLinkedEntitiesProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.LinkedEntitiesProperties;
        protected override List<EntityPropertyInfo> GetLinkedEntityProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.LinkedEntityProperties;
        protected override void FinishEntity(T item) => item.SetDirty(false);
    }
}