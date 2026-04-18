using System.ComponentModel.DataAnnotations;

namespace BECOSOFT.Data.Validation.Attributes {
    public class NotNullAttribute: ValidationAttribute {
        public override bool IsValid(object value) {
            return value != null;
        }
    }
}
