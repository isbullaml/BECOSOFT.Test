using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using System.Collections.Generic;

namespace BECOSOFT.Data.Parsers {
    internal class BaseResultParser<T> : BaseParser<T> where T : BaseResult {
        public BaseResultParser(IOfflineTableExistsRepository tableExistsRepository) : base(tableExistsRepository) {
        }

        protected override List<EntityPropertyInfo> GetInverseLinkedEntityProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.InverseLinkedEntityProperties;
        protected override List<EntityPropertyInfo> GetInverseLinkedBaseChildProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.InverseLinkedBaseChildProperties;
        protected override List<EntityPropertyInfo> GetLinkedEntitiesProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.LinkedBaseResultsProperties;
        protected override List<EntityPropertyInfo> GetLinkedEntityProperties(EntityTypeInfo entityTypeInfo) => entityTypeInfo.LinkedBaseResultProperties;
    }
}