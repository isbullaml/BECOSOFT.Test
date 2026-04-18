using BECOSOFT.Data.Validation;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Validators;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BECOSOFT.ThirdParty.Tests.EuropeanCommission.Validators {
    [TestFixture]
    public class VatNumberValidatorTests {
        private IValidator<VatNumber> _validator;

        [SetUp]
        public void Setup() {
            _validator = new VatNumberValidator();
        }

        [Test]
        public void TestMultipleVatNumbersAreValid() {
            var vatNumbers = new List<VatNumber> {
                new VatNumber("BE0448096349"),
                new VatNumber("DE787525945")
            };
            var result = _validator.Validate(vatNumbers);
            Assert.IsTrue(result.IsValid());
        }

        [Test]
        public void TestIsEuropean() {
            var europeanVat = new VatNumber("BE0448096349");
            _validator.Validate(europeanVat);
            Assert.IsTrue(VatNumberValidator.IsEuropean(europeanVat));
        }

        [Test]
        public void TestValidBelgianVatNumber() {
            var europeanVat = new VatNumber("BE0448096349");
            var result = _validator.Validate(europeanVat);
            Assert.IsTrue(result.IsValid());
        }

        [Test]
        public void TestInvalidBelgianVatNumber() {
            var europeanVat = new VatNumber("BE0468096349");
            var result = _validator.Validate(europeanVat);
            Assert.IsFalse(result.IsValid());
        }

        [Test]
        public void TestInvalidCountryCodeVatNumber() {
            VatNumber vatNumber = new VatNumber("LK0468096349");
            var result = _validator.Validate(vatNumber);
            Assert.IsFalse(result.IsValid());
            Assert.IsTrue(result.Errors.Contains(new ValidationError(nameof(VatNumber.CleanedCountryCode), ThirdParty.Resources.VatNumber_UnknownCountry)));
        }

        [Test]
        public void TestInvalidLengthVatNumber() {
            VatNumber vatNumber = new VatNumber("B");
            var result = _validator.Validate(vatNumber);
            Assert.IsFalse(result.IsValid());
            Assert.IsTrue(result.Errors.Contains(new ValidationError(nameof(VatNumber.VatIdentificationNumber), ThirdParty.Resources.VatNumber_InvalidIdenticationNumberLength)));
        }
    }
}
