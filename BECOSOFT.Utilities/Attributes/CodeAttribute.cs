using System;
using System.ComponentModel;

namespace BECOSOFT.Utilities.Attributes {
   
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CodeAttribute : DescriptionAttribute {
        public CodeAttribute(string description) : base(description) {
        }
    }
}
