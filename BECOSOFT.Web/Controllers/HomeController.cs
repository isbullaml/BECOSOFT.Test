using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Validators;
using BECOSOFT.Web.Helpers;
using BECOSOFT.Web.Models;
using System.Activities.Statements;
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
            if (!ModelState.IsValid)
                return Json(new Result<VatResult>(false, null, Resources.Error_InvalidVatNumber));

            var vatNumber = new VatNumber(model.VatNumber);
            if (!_validator.Validate(vatNumber).IsValid())
                return Json(new Result<VatResult>(false, null, Resources.Error_InvalidVatNumber));

            var result = _service.GetVatNumberInfo(vatNumber);

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

            return Json(new Result<VatResult>(
                true,
                vatResult,
                Resources.Vat_Result
            ));
        }
    }
}