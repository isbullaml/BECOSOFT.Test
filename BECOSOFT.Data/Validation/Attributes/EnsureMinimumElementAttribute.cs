using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace BECOSOFT.Data.Validation.Attributes {
    public class EnsureMinimumElementAttribute : ValidationAttribute {
        private readonly int _minElements;

        public EnsureMinimumElementAttribute(int minElements) {
            _minElements = minElements;
        }

        public override bool IsValid(object value) {
            var list = value as ICollection;
            return list?.Count >= _minElements;
        }
    }
}
