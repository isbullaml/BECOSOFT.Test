using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BECOSOFT.Data.Models.Base {
    public abstract class Dirty : IDirty {
        private HashSet<string> _dirtyPropertyNames;
        protected bool _isDirty;

        /// <summary>
        /// Returns whether the <see cref="Dirty"/> has modified properties.
        /// <para>
        /// When setting <see cref="IsDirty"/> to <see langword="false"/> any internal changes are discarded. If <see cref="IsTrackingChanges"/> was enabled before discarding, <see cref="TrackChanges"/> is called again.
        /// Linked entities are explicitly not included when discarding to prevent discarding changes (on linked entities) that have yet to be saved.
        /// </para>
        /// </summary>
        public virtual bool IsDirty {
            get { return _isDirty; }
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

        /// <summary>
        /// Indicates that property change tracking is enabled.
        /// </summary>
        public bool IsTrackingChanges { get; private set; }

        protected bool IsTrackingLinkedEntities { get; private set; }

        /// <summary>
        /// Returns a <see cref="EntityHashSet{T}"/> of the dirty property names.
        /// </summary>
        /// <returns></returns>
        public EntityHashSet<string> GetDirtyPropertyNames() => new EntityHashSet<string>(_dirtyPropertyNames);

        /// <inheritdoc cref="Dirty.InternalTrackChanges"/>
        /// <para>
        /// If <see cref="BaseEntity.Id"/> is <see langword="0"/>, tracking will not be enabled.
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        /// </para>
        public virtual void TrackChanges(bool includeLinkedEntities = true) => InternalTrackChanges(includeLinkedEntities);

        /// <inheritdoc cref="Dirty.InternalDiscardTrackedChanges"/>
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        public virtual void DiscardTrackedChanges(bool includeLinkedEntities = true) => InternalDiscardTrackedChanges(includeLinkedEntities);

        /// <summary>
        /// Enable property change tracking.
        /// <para>
        /// If <see cref="IsTrackingChanges"/> is <see langword="true"/> tracking will not be enabled again.
        /// </para>
        /// <param name="includeLinkedEntitiesAndChilds">When <see langword="true"/>, property change tracking will also be enabled on all linked entity and linked entities properties</param>
        /// </summary>
        private void InternalTrackChanges(bool includeLinkedEntitiesAndChilds) {
            var wasTrackingChanges = IsTrackingChanges;
            IsTrackingChanges = true;
            if (!wasTrackingChanges) {
                _dirtyPropertyNames = new HashSet<string>();
            }
            IsTrackingLinkedEntities = includeLinkedEntitiesAndChilds;
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var linkedEntitiesProperty in typeInfo.LinkedBaseChildProperties) {
                var entity = (BaseChild)linkedEntitiesProperty.Getter(this);
                entity.TrackChanges();
            }
            if (!includeLinkedEntitiesAndChilds) { return; }
            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntitiesProperties) {
                var list = linkedEntitiesProperty.Getter(this);
                foreach (var entity in (IEnumerable)list) {
                    ((BaseEntity)entity).TrackChanges();
                }
            }

            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntityProperties) {
                var entity = (BaseEntity)linkedEntitiesProperty.Getter(this);
                entity.TrackChanges();
            }
        }

        /// <summary>
        /// Discard the current tracked properties and disables further change tracking.
        /// This does not reset the values of the properties.
        /// <param name="includeLinkedEntities">When <see langword="true"/>, property change tracking will also be disabled on all linked entity and linked entities properties</param>
        /// </summary>
        private void InternalDiscardTrackedChanges(bool includeLinkedEntities) {
            if (!IsTrackingChanges) { return; }
            IsTrackingChanges = false;
            _dirtyPropertyNames = null;
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var linkedEntitiesProperty in typeInfo.LinkedBaseChildProperties) {
                var entity = (BaseChild)linkedEntitiesProperty.Getter(this);
                entity.DiscardTrackedChanges();
            }
            if (!includeLinkedEntities) { return; }
            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntitiesProperties) {
                var list = linkedEntitiesProperty.Getter(this);
                foreach (var entity in (IEnumerable)list) {
                    ((BaseEntity)entity).DiscardTrackedChanges();
                }
            }

            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntityProperties) {
                var entity = (BaseEntity)linkedEntitiesProperty.Getter(this);
                entity.DiscardTrackedChanges();
            }

        }

        /// <summary>
        /// Utility method to help track if the object has been touched (dirty).
        /// </summary>
        /// <typeparam name="T">Type of the field or property</typeparam>
        /// <param name="field">Field</param>
        /// <param name="newValue">New field value</param>
        protected void SetPropertyField<T>(ref T field, T newValue, [CallerMemberName] string callerName = "") {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) {
                return;
            }
            field = newValue;
            IsDirty = true;
            AddDirtyProperty(callerName);
        }

        // For nullable types
        protected void SetProperty<T>(ref T? oldValue, T? newValue, [CallerMemberName] string callerName = "") where T : struct {
            if (oldValue.HasValue != newValue.HasValue || (newValue.HasValue && !EqualityComparer<T>.Default.Equals(oldValue.Value, newValue.Value))) {
                oldValue = newValue;
                IsDirty = true;
                AddDirtyProperty(callerName);
            }
        }

        /// <summary>
        /// Check if any of the properties of the entity has dirty linked entities.
        /// </summary>
        /// <returns>A boolean value</returns>
        public bool HasDirtyLinked() {
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntitiesProperties) {
                var list = linkedEntitiesProperty.Getter(this);
                var dirtyList = list as IDirtyList;
                if (dirtyList != null && dirtyList.IsDirty) {
                    return true;
                }
                foreach (var entity in (IEnumerable) list) {
                    var baseEntity = ((BaseEntity) entity);
                    if (baseEntity.IsDirty) {
                        return true;
                    }
                    if (baseEntity.HasDirtyLinked()) {
                        return true;
                    }
                }
            }

            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntityProperties) {
                var entity = (BaseEntity) linkedEntitiesProperty.Getter(this);
                if (entity.IsDirty) {
                    return true;
                }
                if (entity.HasDirtyLinked()) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// <para>Set the entity's dirtyness (includes linked entities)</para>
        /// <para>This does not set the inverse linked entities, as these will not be saved.</para>
        /// </summary>
        /// <param name="dirty"></param>
        internal void SetDirty(bool dirty) {
            var typeInfo = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var linkedEntitiesProperty in typeInfo.LinkedEntitiesProperties) {
                var list = linkedEntitiesProperty.Getter(this);
                if (list == null) { continue; }
                foreach (var entity in (IEnumerable) list) {
                    ((BaseEntity) entity).SetDirty(dirty);
                }
            }

            foreach (var linkedEntityProperty in typeInfo.LinkedEntityProperties) {
                var entity = (BaseEntity) linkedEntityProperty.Getter(this);
                entity?.SetDirty(dirty);
            }

            foreach (var linkedBaseChildProperty in typeInfo.LinkedBaseChildProperties) {
                var entity = (BaseChild) linkedBaseChildProperty.Getter(this);
                entity?.SetDirty(dirty);
            }

            IsDirty = dirty;
        }

        private void AddDirtyProperty(string propertyName) {
            if (!IsTrackingChanges) { return; }
            if (_dirtyPropertyNames == null) {
                _dirtyPropertyNames = new HashSet<string>();
            }

            _dirtyPropertyNames.Add(propertyName);
        }
    }
}
