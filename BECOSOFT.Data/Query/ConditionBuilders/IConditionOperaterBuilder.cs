using System.Collections.Generic;

namespace BECOSOFT.Data.Query.ConditionBuilders {

    public interface IConditionOperaterBuilder {
        ISetConditionBuilder EqualTo(string parameter);
        ISetConditionBuilder EqualToNull();
        ISetConditionBuilder NotEqualTo(string parameter);
        ISetConditionBuilder NotEqualToNull();
        ISetConditionBuilder LessThan(string parameter);
        ISetConditionBuilder LessThanOrEqualTo(string parameter);
        ISetConditionBuilder GreaterThan(string parameter);
        ISetConditionBuilder GreaterThanOrEqualTo(string parameter);
        ISetConditionBuilder In(IEnumerable<string> parameters);
        ISetConditionBuilder In(string tempTable, string column);
        ISetConditionBuilder In(string subQuery);
        ISetConditionBuilder NotIn(IEnumerable<string> parameters);
        ISetConditionBuilder NotIn(string subQuery);
        ISetConditionBuilder Like(string parameter);
        ISetConditionBuilder Between(string minParameter, string maxParameter);
    }
}
