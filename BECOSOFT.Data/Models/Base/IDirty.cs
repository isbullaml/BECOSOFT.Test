using BECOSOFT.Data.Collections;

namespace BECOSOFT.Data.Models.Base {
    public interface IDirty {
        /// <summary>
        /// Lets the caller know if the entity is dirty of clean
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Indicates that property change tracking is enabled.
        /// </summary>
        bool IsTrackingChanges { get; }

        EntityHashSet<string> GetDirtyPropertyNames();

        /// <inheritdoc cref="Dirty.InternalTrackChanges"/>
        /// <para>
        /// If <see cref="BaseEntity.Id"/> is <see langword="0"/>, tracking will not be enabled.
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        /// </para>
        void TrackChanges(bool includeLinkedEntities = true);

        /// <inheritdoc cref="Dirty.InternalDiscardTrackedChanges"/>
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        void DiscardTrackedChanges(bool includeLinkedEntities = true);
    }
}
