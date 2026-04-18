using BECOSOFT.ThirdParty;
using System.ComponentModel.DataAnnotations;
using System.ServiceModel.Channels;
namespace BECOSOFT.Web.Models
{
    public class VatResult
    {
        public VatResult(string fullAddress)
        {
            FullAddress = fullAddress;
        }
        public string FullAddress { get; set; }
    }
}