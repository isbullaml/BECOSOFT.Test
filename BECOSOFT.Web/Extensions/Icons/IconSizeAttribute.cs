using System;
using System.Collections.Generic;

namespace BECOSOFT.Web.Extensions.Icons {
    public abstract class IconSizeAttribute : Attribute {
        public abstract Dictionary<string, string> GetAttributes();
    }
}