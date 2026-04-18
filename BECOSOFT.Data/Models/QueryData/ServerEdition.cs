using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Linq;

namespace BECOSOFT.Data.Models.QueryData {
    public class ServerEdition : BaseResult, ICacheableResult {
        private string _editionString;

        [Column]
        public string EditionString {
            get => _editionString;
            set {
                _editionString = value;
                SetEdition();
            }
        }

        public SqlServerEdition Edition { get; private set; }

        public bool Is64Bit { get; private set; }

        private void SetEdition() {
            var edition = SqlServerEdition.Unknown;
            if (_editionString.IsNullOrWhiteSpace()) {
                return;
            }
            var split = _editionString.ToSplitList<string>('(');
            var editionIndicator = split[0];
            var editionValues = EnumHelper.GetValues<SqlServerEdition>().Where(se => CodeAttributeHelper.GetCode(se)  != null).ToDictionary(CodeAttributeHelper.GetCode);
            foreach (var editionValue in editionValues) {
                if(!editionIndicator.Contains(editionValue.Key, StringComparison.InvariantCultureIgnoreCase)){ continue; }
                edition = editionValue.Value;
                break;
            }
            Edition = edition;  
            Is64Bit = split.Count > 1 && split[1].Contains("64");
        }

        /// <summary>
        /// Indicates that the server edition is an Express Edition variant
        /// </summary>
        public bool IsExpress => Edition == SqlServerEdition.Express || Edition == SqlServerEdition.ExpressWithAdvancedServices;
    }
}
