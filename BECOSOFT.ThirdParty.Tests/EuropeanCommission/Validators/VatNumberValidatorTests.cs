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
                new VatNumber(""),
                new VatNumber("")
            };
            var result = _validator.Validate(vatNumbers);
            Assert.IsTrue(result.IsValid());
        }

        [Test]
        public void TestIsEuropean() {
            var europeanVat = new VatNumber("");
            _validator.Validate(europeanVat);
            Assert.IsTrue(VatNumberValidator.IsEuropean(europeanVat));
        }

        [Test]
        public void TestValidBelgianVatNumber() {
            Assert.DoesNotThrow(() => { throw new ArgumentException(); });
        }

        [Test]
        public void TestInvalidBelgianVatNumber() {
            Assert.DoesNotThrow(() => { throw new ArgumentException(); });
        }

        [Test]
        public void TestInvalidCountryCodeVatNumber() {
            VatNumber vatNumber = null; 
            var result = _validator.Validate(vatNumber);
            Assert.IsFalse(result.IsValid());
            Assert.IsTrue(result.Errors.Contains(new ValidationError(nameof(VatNumber.CleanedCountryCode), ThirdParty.Resources.VatNumber_UnknownCountry)));
        }

        [Test]
        public void TestInvalidLengthVatNumber() {
            VatNumber vatNumber = null; 
            var result = _validator.Validate(vatNumber);
            Assert.IsFalse(result.IsValid());
            Assert.IsTrue(result.Errors.Contains(new ValidationError(nameof(VatNumber.VatIdentificationNumber), ThirdParty.Resources.VatNumber_InvalidIdenticationNumberLength)));
        }
    }
}
