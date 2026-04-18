using BECOSOFT.Data.Converters;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.Base {
    /// <summary>
    /// BaseEntity represents an entity with an identity.
    /// This class should map to a table with a primary key (the Id field).
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class BaseEntity : Dirty, IValidatable, IEntity {


        /// <summary>
        /// Returns whether the <see cref="Dirty"/> has modified properties.
        /// <para>
        /// When setting <see cref="IsDirty"/> to <see langword="false"/> any internal changes are discarded. If <see cref="Dirty.IsTrackingChanges"/> was enabled before discarding, <see cref="TrackChanges"/> is called again.
        /// Linked entities are explicitly not included when discarding to prevent discarding changes (on linked entities) that have yet to be saved.
        /// </para>
        /// </summary>
        public override bool IsDirty {
            get => Id == 0 || _isDirty || HasDirtyBaseChildren();
            protected internal set {
                _isDirty = value;
                if (_isDirty) { return; }
                var isTracking = IsTrackingChanges;
                DiscardTrackedChanges(false);
                if (isTracking) {
                    TrackChanges(IsTrackingLinkedEntities);
                }
            }
        }

        public int Id { get; set; }

        /// <inheritdoc cref="Dirty.InternalTrackChanges"/>
        /// <para>
        /// If <see cref="Id"/> is <see langword="0"/>, tracking will not be enabled.
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be enabled on all linked entity and linked entities properties</param>
        /// </para>
        public override void TrackChanges(bool includeLinkedEntities = true) {
            if (Id == 0) { return; }
            base.TrackChanges(includeLinkedEntities);
        }

        /// <inheritdoc cref="Dirty.InternalDiscardTrackedChanges"/>
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be enabled on all linked entity and linked entities properties</param>
        public override void DiscardTrackedChanges(bool includeLinkedEntities = true) {
            if (Id == 0) { return; }
            base.DiscardTrackedChanges(includeLinkedEntities);
        }

        protected bool Equals(BaseEntity other) {
            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((BaseEntity) obj);
        }

        public override int GetHashCode() {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Id;
        }

        public static bool operator ==(BaseEntity left, BaseEntity right) {
            return Equals(left, right);
        }

        public static bool operator !=(BaseEntity left, BaseEntity right) {
            return !(left == right);
        }

        public string ToValidationLogString() {
            return Id.ToString();
        }

        private bool HasDirtyBaseChildren() {
            var info = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var property in info.Properties) {
                if (!property.IsBaseChild) { continue; }
                var baseChild = (BaseChild) property.Getter(this);
                if (baseChild?.IsDirty ?? false) {
                    return true;
                }
            }

            return false;
        }

        private string DebuggerDisplay => $"{Id}";
    }
}
