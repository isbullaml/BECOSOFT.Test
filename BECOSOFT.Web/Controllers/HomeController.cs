using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Validators;
using BECOSOFT.Web.Helpers;
using BECOSOFT.Web.Models;
using System.Web.Mvc;

namespace BECOSOFT.Web.Controllers {
    public class HomeController : Controller {
        private IVatNumberValidationService _service;
        private IViesValidationService _viesValidationService;
        private VatNumberValidator _validator;
        public HomeController()
        {
            _viesValidationService = new ViesValidationService();
            _validator = new VatNumberValidator();
            _service = new VatNumberValidationService(_validator, _viesValidationService);
        }
        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        public JsonResult Search(VatSearchViewModel model)
        {
            var vatNumber = new VatNumber(model.VatNumber);
            if (!_validator.Validate(vatNumber).IsValid()) return Json(new Result<VatResult>(false, null, Resources.Error_InvalidVatNumber));

            var result = _service.GetVatNumberInfo(vatNumber);
            if(!result.IsValid) return Json(new Result<VatResult>(false,null,Resources.Error_VatNumberNotExist));
            var name = result.ViesResponse?.Name;

            var address = result.ViesResponse?.Address;

            var street = $"{address?.Street} {address?.Number}".Trim();
            var place = $"{address?.PostalCode} {address?.Place}".Trim();
            var country = CountryHelper.GetCountryName(address.CountryCode);

            var fullAddress = string.Format(Resources.Vat_Result, $"{name} / {street}, {place}, {country}");

            return Json(new Result<VatResult>(true, new VatResult(fullAddress), Resources.Vat_Result));
        }
    }
}