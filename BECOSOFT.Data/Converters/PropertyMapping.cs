using System.Collections.Generic;

namespace BECOSOFT.Data.Converters {
    internal class PropertyMapping {
        public EntityPropertyInfo PropertyInfo { get; set; }
        public int Index { get; set; }
        public List<PropertyMapping> ChildMappings { get; private set; }
        public DelegateCreator.Projector BaseChildProjector { get; private set; }


        public PropertyMapping(EntityPropertyInfo propertyInfo) {
            PropertyInfo = propertyInfo;
        }

        public void SetChildMappings(List<PropertyMapping> childMappings) {
            ChildMappings = childMappings;
            BaseChildProjector = DelegateCreator.CreateConvertEntityDelegate(PropertyInfo.PropertyType);
        }
    }
}