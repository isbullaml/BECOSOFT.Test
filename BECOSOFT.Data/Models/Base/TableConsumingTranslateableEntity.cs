using BECOSOFT.Data.Collections;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Data.Models.Base {
    public abstract class TableConsumingTranslateableEntity<T, TDefining> : TableConsumingEntity<TDefining>
        where T : TableConsumingTranslationEntity<TDefining> 
        where TDefining : TableDefiningEntity {
        public abstract ObserverList<T> Translations { get; set; }

        public abstract void AddTranslation(int languageID, string translated);

        public void AddTranslations(IEnumerable<T> translations) {
            foreach (var translation in translations) {
                AddTranslation(translation);
            }
        }

        public void AddTranslation(T translation) {
            if (Translations.Any(t => t.LanguageID == translation.LanguageID)) {
                RemoveTranslation(translation.LanguageID);
            }
            Translations.Add(translation);
        }

        public T GetTranslation(int languageID, int defaultLanguageID = 0) {
            if (Translations.IsEmpty()) { return null; }
            var translation = Translations.FirstOrDefault(t => t.LanguageID == languageID);
            if (translation == null && defaultLanguageID != 0) {
                translation = Translations.FirstOrDefault(t => t.LanguageID == defaultLanguageID);
            }
            return translation ?? Translations.FirstOrDefault();
        }

        public void RemoveTranslation(int languageID) {
            Translations.RemoveAll(t => t.LanguageID == languageID);
        }
    }
}