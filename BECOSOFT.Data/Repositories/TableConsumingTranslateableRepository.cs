using BECOSOFT.Data.Context;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Repositories {
    public abstract class TableConsumingTranslateableRepository<T, TTranslation, TDefining> : Repository<T>
        where T : TableConsumingTranslateableEntity<TTranslation, TDefining>
        where TTranslation : TableConsumingTranslationEntity<TDefining>
        where TDefining : TableDefiningEntity {
        private readonly ITableConsumingTranslationEntityRepository<TTranslation, TDefining> _translationRepository;

        protected TableConsumingTranslateableRepository(IDbContextFactory dbContextFactory,
                                                        IDatabaseCommandFactory databaseCommandFactory, 
                                                        ITableConsumingTranslationEntityRepository<TTranslation, TDefining> translationRepository) 
            : base(dbContextFactory, databaseCommandFactory) {
            _translationRepository = translationRepository;
        }

        public override void Save(IEnumerable<T> entities, string tablePart = null) {
            var entityList = entities.ToSafeList();
            base.Save(entityList, tablePart);
            var translationList = new List<TTranslation>();
            var entityIDsFromChangedTranslations = new List<int>();
            var translationIDs = new List<int>();
            foreach (var entity in entityList) {
                var id = entity.Id;
                if (entity.Translations.IsDirty) {
                    translationIDs.AddRange(entity.Translations.GetIDs());
                    entity.Translations.CleanDirty();
                    entityIDsFromChangedTranslations.Add(id);
                }
                foreach (var translation in entity.Translations) {
                    if (!translation.IsDirty) {
                        continue;
                    }
                    translation.ParentID = id;
                    translationList.Add(translation);
                }
            }
            translationIDs = translationIDs.Where(id => id != 0).ToList();
            _translationRepository.DeleteNotIn(translationIDs, entityIDsFromChangedTranslations, (TTranslation e) => e.ParentID, tablePart);
            _translationRepository.Save(translationList, tablePart);
        }

        public override void Delete(IEnumerable<int> ids, string tablePart = null) {
            var idList = ids.ToSafeList();
            if (idList.IsEmpty()) { return; }
            _translationRepository.DeleteNotIn(null, idList, (TTranslation e) => e.ParentID, tablePart);
            base.Delete(idList, tablePart);
        }
    }
}