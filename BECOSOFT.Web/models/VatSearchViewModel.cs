using BECOSOFT.ThirdParty;
using System.ComponentModel.DataAnnotations;
namespace BECOSOFT.Web.Models
{
    public class VatSearchViewModel
    {
        [Required(ErrorMessageResourceType = typeof(BECOSOFT.Core.Resources), ErrorMessageResourceName = nameof(BECOSOFT.Core.Resources.General_IsRequired))]
        [Display(Name = nameof(Resources.Vat_Number), ResourceType = typeof(Resources))]
        public string VatNumber { get; set; }
    }
}