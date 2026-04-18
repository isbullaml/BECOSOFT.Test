using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Repositories {
    public abstract class Repository<T> : ReadonlyRepository<T>, IRepository<T> where T : BaseEntity {

        protected Repository(IDbContextFactory dbContextFactory, 
                             IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
        }

        public void Save(T entity, string tablePart = null) {
            var entityList = new List<T> { entity };
            Save(entityList, tablePart);
        }

        public virtual void Save(IEnumerable<T> entities, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityList = entities.ToSafeList();
            if (entityList.IsEmpty()) {
                return;
            }
            var entitiesToInsert = new List<T>(entityList.Count);
            var entitiesToUpdate = new List<T>(entityList.Count);
            foreach (var entity in entityList) {
                if (entity.Id == 0) {
                    entitiesToInsert.Add(entity);
                } else {
                    if (!entity.IsDirty) {
                        continue;
                    }
                    entitiesToUpdate.Add(entity);
                }
            }
            using (var context = GetContext()) {
                if (entitiesToInsert.HasAny()) {
                    var commandBuilder = DatabaseCommandFactory.Insert(entitiesToInsert, tablePart);
                    context.Insert(commandBuilder);
                }
                if (entitiesToUpdate.HasAny()) {
                    var commandBuilder = DatabaseCommandFactory.Update(entitiesToUpdate, null, tablePart);
                    context.Update(commandBuilder);
                }
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public void UpdateProperty(T entity, IEnumerable<EntityPropertyInfo> selectedProperties, string tablePart = null) {
            var entityList = new List<T> { entity };
            UpdateProperty(entityList, selectedProperties, tablePart);
        }

        public void UpdateProperty(IEnumerable<T> entities, IEnumerable<EntityPropertyInfo> properties, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var updateableProperties = properties.Where(p => p.Updateable).ToList();
            if (updateableProperties.IsEmpty()) { return;}
            var entityList = entities.ToSafeList();
            if (entityList.IsEmpty()) {
                return;
            }
            var entitiesToUpdate = new List<T>(entityList.Count);
            foreach (var entity in entityList) {
                if (entity.Id == 0 || !entity.IsDirty) {
                    continue;
                }
                entitiesToUpdate.Add(entity);
            }
            if (entitiesToUpdate.IsEmpty()) { return; }
            var command = DatabaseCommandFactory.Update(entitiesToUpdate, updateableProperties, tablePart);
            using (var context = GetContext()) {
                context.Update(command);
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public void UpdateProperty<TProp>(int id, EntityPropertyInfo property, TProp value, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            if (id == 0) { return; }
            if (!property.Updateable) { return; }
            var command = DatabaseCommandFactory.Update<T, TProp>(id, property, value, tablePart);
            using (var context = GetContext()) {
                context.UpdateProperty(command);
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public void UpdateProperty<TProp>(IEnumerable<int> ids, EntityPropertyInfo property, TProp value, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            if (!property.Updateable) { return; }
            var filteredIDs = ids.Where(id => id != 0).ToSafeList();
            if (filteredIDs.IsEmpty()) { return; }
            var command = DatabaseCommandFactory.Update<T, TProp>(filteredIDs, property, value, tablePart);
            using (var context = GetContext()) {
                context.UpdateProperty(command);
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public void Delete(T entity, string tablePart = null) {
            Delete(entity.Id, tablePart);
        }

        public void Delete(int id, string tablePart = null) {
            var ids = new List<int> { id };
            Delete(ids, tablePart);
        }

        public virtual void Delete(IEnumerable<int> ids, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var idList = ids.ToSafeList();
            if (idList.IsEmpty()) { return; }
            var command = DatabaseCommandFactory.Delete<T>(t => idList.Contains(t.Id), tablePart);
            using (var context = GetContext()) {
                context.Delete(command);
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public virtual void DeleteBy(Expression<Func<T, bool>> where, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var command = DatabaseCommandFactory.Delete(where, tablePart);
            using (var context = GetContext()) {
                context.Delete(command);
            }
            if (UseCaching) { Cache.ClearCache(); }
        }

        public DeleteResult CanDelete(int id, string tablePart = null) {
            return CanDelete(new List<int> { id }, tablePart);
        }

        public virtual DeleteResult CanDelete(IEnumerable<int> entityIDs, string tablePart = null) {
            var result = new KeyValueList<int, EntityDeleteResult>();
            foreach (var entityID in entityIDs) {
                result.Add(entityID, new EntityDeleteResult(true));
            }
            return new DeleteResult(result);
        }
    }
}