using BECOSOFT.Utilities.Collections;
using System.Linq;

namespace BECOSOFT.Data.Models.Base {
    public class DeleteResult {
        public KeyValueList<int, EntityDeleteResult> Result { get; }

        public bool AreAllDeleteable => Result.All(r => r.Value.IsDeleteable);

        internal DeleteResult(KeyValueList<int, EntityDeleteResult> result) {
            Result = result;
        }

        public EntityDeleteResult GetEntityResult(int id) {
            return Result.FirstOrDefault(res => res.Key == id).Value;
        }

        public static DeleteResult Empty() {
            return new DeleteResult(new KeyValueList<int, EntityDeleteResult>(0));
        }
    }
}