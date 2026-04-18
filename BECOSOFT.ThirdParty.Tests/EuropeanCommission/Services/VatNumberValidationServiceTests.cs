using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.ThirdParty.EuropeanCommission.Validators;
using NUnit.Framework;

namespace BECOSOFT.ThirdParty.Tests.EuropeanCommission.Services {
    [TestFixture]
    public class VatNumberValidationServiceTests {
        private const string BelgianAddressSeparator = "\n";

        private IVatNumberValidationService _service;
        private IViesValidationService _viesValidationService;
        private VatNumberValidator _validator;

        [SetUp]
        public void Setup() {
            _viesValidationService = new ViesValidationService();
            _validator = new VatNumberValidator();
            _service = new VatNumberValidationService(_validator, _viesValidationService);
        }

        [Test]
        public void TestValidBelgianVatNumber() {
            var vatNumber = new VatNumber("BE 0448.096.349");

            var result = _service.GetVatNumberInfo(vatNumber);
            if (!CanAssertResult(result)) {
                return;
            }

            Assert.IsTrue(result.IsValid);
            var address = result.ViesResponse?.Address;
            Assert.IsNotNull(address);
            Assert.AreEqual("BE0448096349", result.VatNumber.ValidatedVatNumber);
            Assert.AreEqual("BE", result.VatNumber.CleanedCountryCode);
            Assert.AreEqual("BVBA BECOSOFT", result.ViesResponse.Name);
            Assert.AreEqual($"Luxemburgstraat 1{BelgianAddressSeparator}9140 Temse", address.Address);
            Assert.AreEqual("Luxemburgstraat", address.Street);
            Assert.AreEqual("1", address.Number);
            Assert.AreEqual("9140", address.PostalCode);
            Assert.AreEqual("Temse", address.Place);
        }

        [Test]
        public void TestValidBelgianVatNumberWithMultipleStreetParts() {
            var vatNumber = new VatNumber("BE 0406.383.379");

            var result = _service.GetVatNumberInfo(vatNumber);
            if (!CanAssertResult(result)) {
                return;
            }

            Assert.IsTrue(result.IsValid);
            var address = result.ViesResponse?.Address;
            Assert.IsNotNull(address);
            Assert.AreEqual("BE0406383379", result.VatNumber.ValidatedVatNumber);
            Assert.AreEqual("BE", result.VatNumber.CleanedCountryCode);
            Assert.AreEqual("BV FedEx Express BE", result.ViesResponse.Name);
            Assert.AreEqual($"Bedrijvenzone Machelen-Cargo 711{BelgianAddressSeparator}1830 Machelen (Brab.)", address.Address);
            Assert.AreEqual("Bedrijvenzone Machelen-Cargo", address.Street);
            Assert.AreEqual("711", address.Number);
            Assert.AreEqual("1830", address.PostalCode);
            Assert.AreEqual("Machelen (Brab.)", address.Place);
        }

        [Test]
        public void TestValidBelgianVatNumberWithBox() {
            var vatNumber = new VatNumber("BE 0848.642.013");

            var result = _service.GetVatNumberInfo(vatNumber);
            if (!CanAssertResult(result)) {
                return;
            }

            Assert.IsTrue(result.IsValid);
            var address = result.ViesResponse?.Address;
            Assert.IsNotNull(address);
            Assert.AreEqual("BE0848642013", result.VatNumber.ValidatedVatNumber);
            Assert.AreEqual("BE", result.VatNumber.CleanedCountryCode);
            Assert.AreEqual("CVBA LATRI", result.ViesResponse.Name);
            Assert.AreEqual($"Maenhoutstraat 12/1{BelgianAddressSeparator}9830 Sint-Martens-Latem", address.Address);
            Assert.AreEqual("Maenhoutstraat", address.Street);
            Assert.AreEqual("12/1", address.Number);
            Assert.AreEqual("", address.Box);
            Assert.AreEqual("9830", address.PostalCode);
            Assert.AreEqual("Sint-Martens-Latem", address.Place);
        }

        [Test]
        public void TestValidBelgianVatNumber2() {
            var vatNumber = new VatNumber("BE0598900467");

            var result = _service.GetVatNumberInfo(vatNumber);
            if (!CanAssertResult(result)) {
                return;
            }

            Assert.IsTrue(result.IsValid);
            var address = result.ViesResponse?.Address;
            Assert.IsNotNull(address);
            Assert.AreEqual("BE0598900467", result.VatNumber.ValidatedVatNumber);
            Assert.AreEqual("BE", result.VatNumber.CleanedCountryCode);
            Assert.AreEqual("ENT E Toshiba TEC Germany Imaging Systems", result.ViesResponse.Name);
            Assert.AreEqual($"Z. 1 Researchpark 160{BelgianAddressSeparator}1731 Asse", address.Address);
            Assert.AreEqual("Z. 1 Researchpark", address.Street);
            Assert.AreEqual("160", address.Number);
            Assert.AreEqual("", address.Box);
            Assert.AreEqual("1731", address.PostalCode);
            Assert.AreEqual("Asse", address.Place);
        }

        // In case VIES web service is down.
        private static bool CanAssertResult(VatNumberInfo result) {
            if (result.HasViesResponse) {
                return !(result.ViesResponse.HasError && (ViesErrorResponse.RetryableErrors.Contains(result.ViesResponse.Error) || result.ViesResponse.Error == ViesErrorResponse.MemberStateUnavailable.Error));
            }

            return true;
        }
    }
}
