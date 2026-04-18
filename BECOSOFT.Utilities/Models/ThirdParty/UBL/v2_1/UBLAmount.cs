using System.Globalization;
using System.Xml.Serialization;

namespace BECOSOFT.Utilities.Models.ThirdParty.UBL.v2_1 {
    public class UBLAmount {
        [XmlText]
        public string AmountString {
            get => Amount.ToString(CultureInfo.InvariantCulture);
            set {
                decimal temp;
                decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out temp);
                Amount = temp;
            }
        }

        [XmlIgnore]
        public decimal Amount { get; set; }

        [XmlAttribute("currencyID")]
        public string CurrencyID { get; set; }

        public UBLAmount() {
        }

        public UBLAmount(decimal amount, string currencyID) {
            Amount = amount;
            CurrencyID = currencyID;
        }
    }
}