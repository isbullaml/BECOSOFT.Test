using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Repositories;

namespace BECOSOFT.Data.Models.Base {
    /// <summary>
    /// An entity that has only one property(<see cref="PrimaryKeyEntity.Id"/>).
    /// This entity is used by the <see cref="PrimaryKeyRepository"/>.
    /// It can also be used for anonymous object mocking where an entity with an <see cref="PrimaryKeyEntity.Id"/> property.
    /// </summary>
    [Table(Schema.Unknown, "", "Id")]
    internal sealed class PrimaryKeyEntity : BaseEntity {
    }
}