using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models {
    public sealed class PrimaryKeyContainer : IPrimaryKeyContainer {
        private readonly Dictionary<PrimaryKeyType, HashSet<int>> _primaryKeyTypeContainer = new Dictionary<PrimaryKeyType, HashSet<int>>(0);

        public int Count => _primaryKeyTypeContainer.Count;

        public bool IsEmpty() => Count == 0;

        public HashSet<int> TryGetIDs<T>(string tablePart = null) {
            return TryGetIDs(typeof(T), tablePart);
        }

        public HashSet<int> TryGetIDs(Type type, string tablePart = null) {
            Check.IsValidTableConsuming(type, tablePart);
            var primaryKeyType = new PrimaryKeyType(type, tablePart);
            return _primaryKeyTypeContainer.TryGetValueWithDefault(primaryKeyType) ?? new HashSet<int>();
        }

        public void Add(PrimaryKeyType type, int id) {
            Check.IsValidTableConsuming(type.Type, type.TablePart);
            if (_primaryKeyTypeContainer.TryGetValue(type, out var existingIDs)) {
                existingIDs.Add(id);
            } else {
                _primaryKeyTypeContainer.Add(type, new HashSet<int> { id });
            }
        }

        public void Add(PrimaryKeyType type, IEnumerable<int> ids) {
            Check.IsValidTableConsuming(type.Type, type.TablePart);
            if (_primaryKeyTypeContainer.TryGetValue(type, out var existingIDs)) {
                existingIDs.AddRange(ids);
            } else {
                _primaryKeyTypeContainer.Add(type, ids.ToSafeHashSet());
            }
        }

        public void Add(Type type, int id, string tablePart = null) {
            var primaryKeyType = new PrimaryKeyType(type, tablePart);
            Add(primaryKeyType, id);
        }

        public void Add(Type type, IEnumerable<int> ids, string tablePart = null) {
            var primaryKeyType = new PrimaryKeyType(type, tablePart);
            Add(primaryKeyType, ids);
        }

        public void Add<T>(int id, string tablePart = null) where T : BaseEntity {
            Add(typeof(T), id, tablePart);
        }

        public void Add<T>(IEnumerable<int> ids, string tablePart = null) where T:BaseEntity {
            Add(typeof(T), ids, tablePart);
        }

        public IEnumerator<KeyValuePair<PrimaryKeyType, HashSet<int>>> GetEnumerator() {
            return _primaryKeyTypeContainer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}