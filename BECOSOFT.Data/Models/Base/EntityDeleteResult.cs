namespace BECOSOFT.Data.Models.Base {
    public class EntityDeleteResult {
        public bool IsDeleteable { get; set; }
        public string Reason { get; set; }

        public EntityDeleteResult(bool isDeleteable, string reason = null) {
            IsDeleteable = isDeleteable;
            Reason = reason;
        }
    }
}