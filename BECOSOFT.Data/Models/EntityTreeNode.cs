using BECOSOFT.Data.Converters;
using System.Diagnostics;

namespace BECOSOFT.Data.Models {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class EntityTreeNode {
        internal int Index { get; }
        internal EntityTypeInfo EntityTypeInfo { get; }
        internal EntityPropertyInfo EntityPropertyInfo { get; }
        internal EntityTreeNodeType Type { get; }

        /// <inheritdoc />
        public EntityTreeNode(EntityTypeInfo entityTypeInfo) : this(0, entityTypeInfo, null, EntityTreeNodeType.Base) {
        }

        /// <inheritdoc />
        public EntityTreeNode(int index, EntityTypeInfo entityTypeInfo,
                              EntityPropertyInfo entityPropertyInfo,
                              EntityTreeNodeType type) {
            Index = index;
            EntityTypeInfo = entityTypeInfo;
            EntityPropertyInfo = entityPropertyInfo;
            Type = type;
        }

        private string DebuggerDisplay => $"{Index} {Type} - {EntityTypeInfo.EntityType.Name} ({Type})";
    }

    internal enum EntityTreeNodeType {
        Base,
        LinkedEntity,
        LinkedEntities,
        InverseLinkedEntity,
    }
}
