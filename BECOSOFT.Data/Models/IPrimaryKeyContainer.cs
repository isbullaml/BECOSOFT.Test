using BECOSOFT.Data.Models.Base;
using System;
using System.Collections.Generic;

namespace BECOSOFT.Data.Models {
    public interface IPrimaryKeyContainer : IReadonlyPrimaryKeyContainer {
        void Add(PrimaryKeyType type, int id);
        void Add(PrimaryKeyType type, IEnumerable<int> ids);
        void Add(Type type, int id, string tablePart = null);
        void Add(Type type, IEnumerable<int> ids, string tablePart = null);
        void Add<T>(int id, string tablePart = null) where T : BaseEntity;
        void Add<T>(IEnumerable<int> ids, string tablePart = null) where T : BaseEntity;
    }
}