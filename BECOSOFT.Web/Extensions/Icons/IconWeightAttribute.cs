using System;
using System.Collections.Generic;

namespace BECOSOFT.Web.Extensions.Icons {
    public abstract class IconWeightAttribute : Attribute {
        public abstract Dictionary<string, string> GetAttributes();
    }
}