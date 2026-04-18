namespace BECOSOFT.Data.Models.Base {
    public abstract class TableDefiningEntity : BaseEntity {
        public abstract string TableName { get; set; }
    }
}