using BECOSOFT.Utilities.Converters.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BECOSOFT.Utilities.Models.Mapping.Filters {
    [JsonConverter(typeof(JsonSubtypes))]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(FilterGroupingCondition), "LogicalGrouping")]
    [JsonSubtypes.KnownSubTypeWithProperty(typeof(FilterPropertyCondition), "Entity")]
    public abstract class FilterCondition {

        public abstract bool IsValid();
    }
    
    public class FilterConditionContainer {
        public FilterCondition PreselectionCondition { get; set; }
        public FilterCondition Condition { get; set; }

        public static JsonSerializerSettings GetSettings() {
            var settings = new JsonSerializerSettings {
                Formatting = Formatting.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return settings;
        }

        public static FilterConditionContainer Empty() {
            return new FilterConditionContainer {
                Condition = new FilterGroupingCondition(),
                PreselectionCondition = new FilterGroupingCondition(),
            };
        }

        [JsonIgnore]
        public string JsonData => JsonConvert.SerializeObject(this, GetSettings());
    }

    // ReSharper disable once UnusedTypeParameter
    public class FilterContainerParameters<T> {
        public int LanguageID { get; set; }
        public bool SkipPossibleValues { get; set; }
    }
}