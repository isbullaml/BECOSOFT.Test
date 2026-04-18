using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services {
    public class ViesValidationService : IViesValidationService {
        private Dictionary<string, ViesValidationResponse> _preDefinedVatNumbers = new Dictionary<string, ViesValidationResponse> {
            { "BE0448096349", new ViesValidationResponse("BE", "0448096349", DateTime.Now, true, "BVBA BECOSOFT", "Luxemburgstraat 1\n9140 Temse") },
            { "BE0406383379", new ViesValidationResponse("BE", "0406383379", DateTime.Now, true, "BV FedEx Express BE", "Bedrijvenzone Machelen-Cargo 711\n1830 Machelen (Brab.)") },
            { "BE0848642013", new ViesValidationResponse("BE", "0848642013", DateTime.Now, true, "CVBA LATRI", "Maenhoutstraat 12/1\n9830 Sint-Martens-Latem") },
            { "BE0598900467", new ViesValidationResponse("BE", "0598900467", DateTime.Now, true, "ENT E Toshiba TEC Germany Imaging Systems", $"Z. 1 Researchpark 160\n1731 Asse") },
        };

        public ViesValidationResponse Validate(VatNumber vatNumber) {
            var validationResult = Task.Run(() => ValidateAsync(vatNumber));
            return validationResult.Result;
        }

        public async Task<ViesValidationResponse> ValidateAsync(VatNumber vatNumber) {
            await Task.Delay(500);
            return _preDefinedVatNumbers.TryGetValueWithDefaultFunc(vatNumber.CleanedVatNumber, () => new ViesValidationResponse("ERROR", "ERROR"));
        }
    }
}