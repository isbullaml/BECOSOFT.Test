using BECOSOFT.Data.Validation;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.Web.Helpers;
using BECOSOFT.Web.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BECOSOFT.Web.Controllers {
    public class HomeController : Controller {
        private readonly IVatNumberCacheService _vatNumberCacheService;
        private readonly IValidator<VatNumber> _validator;

        public HomeController(IVatNumberCacheService vatNumberCacheService,
                              IValidator<VatNumber> validator) {
            _vatNumberCacheService = vatNumberCacheService;
            _validator = validator;
        }

        public ActionResult Index() => View();

        [HttpPost]
        public async Task<JsonResult> Search(VatSearchViewModel model) {
            if (!ModelState.IsValid)
                return Json(new Result<VatResult>(false, null, Resources.Error_InvalidVatNumber));

            var vatNumber = new VatNumber(model.VatNumber);
            if (!_validator.Validate(vatNumber).IsValid())
                return Json(new Result<VatResult>(false, null, Resources.Error_InvalidVatNumber));

            var result = await _vatNumberCacheService.GetVatNumberInfoAsync(vatNumber);

            if (!result.IsValid)
                return Json(new Result<VatResult>(false, null, Resources.Error_VatNumberNotExist));

            var vies = result.ViesResponse;
            var vatResult = new VatResult(
                name: vies?.Name,
                street: vies?.Address?.Street,
                number: vies?.Address?.Number,
                postalCode: vies?.Address?.PostalCode,
                place: vies?.Address?.Place,
                countryCode: vies?.Address?.CountryCode
            );

            return Json(new Result<VatResult>(true, vatResult, Resources.Vat_Result));
        }
    }
}
