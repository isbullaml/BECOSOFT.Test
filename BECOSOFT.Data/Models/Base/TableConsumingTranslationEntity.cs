using BECOSOFT.Data.Attributes;

namespace BECOSOFT.Data.Models.Base {
    public abstract class TableConsumingTranslationEntity<TDefining> : TableConsumingEntity<TDefining> where TDefining : TableDefiningEntity {
        // ReSharper disable once InconsistentNaming
        protected int _parentID;
        private int _languageID;

        [NotSearchable]
        public abstract int ParentID { get; set; }

        [Column]
        [NotSearchable]
        public int LanguageID {
            get { return _languageID; }
            set { SetPropertyField(ref _languageID, value); }
        }
    }
}