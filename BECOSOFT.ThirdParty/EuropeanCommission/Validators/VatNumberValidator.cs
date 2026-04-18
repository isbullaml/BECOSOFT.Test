using BECOSOFT.Data.Validation;
using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Models;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Validators {
    public sealed class VatNumberValidator : Validator<VatNumber> {
        private static readonly Dictionary<string, List<Regex>> EuropeanVatNumberFormatRegexes = FillEuropeanFormatRegexes();
        private static readonly Dictionary<string, Func<VatNumber, bool>> EuropeanVatNumberCheckFunctions = FillCheckFunctions();
        private static readonly Dictionary<string, List<Regex>> VatNumberFormatRegexes = FillFormatRegexes();

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public static readonly HashSet<string> EuropeanCountryCodes = EuropeanVatNumberFormatRegexes.Keys.ToSafeHashSet();
        public static readonly HashSet<string> NonEuropeanCountryCodes = VatNumberFormatRegexes.Keys.ToSafeHashSet();
        public static readonly HashSet<string> KnownCountryCodes = (EuropeanCountryCodes.Union(NonEuropeanCountryCodes)).ToSafeHashSet();
        private readonly HashSet<string> _formatsWithSpaces = new HashSet<string> { "DK" };
        private readonly HashSet<string> _formatsWithHyphens = new HashSet<string> { "CL", "GT" };

        public static bool IsEuropean(VatNumber vatNumber) {
            return EuropeanCountryCodes.Contains(vatNumber.CleanedCountryCode);
        }

        public VatNumberValidator() : base(Logger, null) {
        }

        protected override void ExtraValidation(IValidationContainer<VatNumber> container) {
            var entities = container.Entities;
            var errors = container.ErrorList;
            for (var index = 0; index < entities.Count; index++) {
                var entity = entities[index];
                ValidateVatNumber(entity, errors[index]);
            }
        }

        private void ValidateVatNumber(VatNumber entity, IErrorList errors) {
            if (!HasValidLength(entity)) {
                errors.Add(nameof(entity.VatIdentificationNumber), Resources.VatNumber_InvalidIdenticationNumberLength);
                return;
            }
            Clean(entity);
            if (!KnownCountryCodes.Contains(entity.CountryCode)) {
                errors.Add(nameof(entity.CleanedCountryCode), Resources.VatNumber_UnknownCountry);
                return;
            }
            var isValidFormatRegex = HasValidFormatRegex(entity);
            if (!isValidFormatRegex) {
                errors.Add(nameof(entity.VatIdentificationNumber), Resources.VatNumber_UnknownFormat);
                return;
            }
            var isValidFormatCheck = HasValidFormatCheck(entity);
            if (!isValidFormatCheck) {
                errors.Add(nameof(entity.VatIdentificationNumber), Resources.VatNumber_InvalidFormat);
            }
        }

        private static bool HasValidLength(VatNumber entity) {
            var vatNumber = entity.VatIdentificationNumber;
            return !string.IsNullOrWhiteSpace(vatNumber) && vatNumber.Length >= 2;
        }

        private void Clean(VatNumber entity) {
            var vatNumber = entity.VatIdentificationNumber;
            var spacesAllowed = _formatsWithSpaces.Contains(entity.CountryCode);
            var hyphensAllowed = _formatsWithHyphens.Contains(entity.CountryCode);
            var isIreland = entity.CountryCode.Equals("IE");
            var cleaned = new StringBuilder();
            for (var i = 0; i < vatNumber.Length; i++) {
                var character = vatNumber[i];
                var addChar = char.IsLetterOrDigit(character)
                              || (i > 1 && spacesAllowed && character == ' ')
                              || (isIreland && (character == '+' || character == '*'))
                              || (hyphensAllowed && character == '-');
                if (addChar) {
                    cleaned.Append(character);
                }
            }
            entity.CleanedVatNumber = cleaned.ToString();
        }

        public bool HasValidFormatRegex(VatNumber entity) {
            var vatNumber = entity.CleanedVatNumber;
            var countryPart = entity.CleanedCountryCode;

            var regexes = EuropeanVatNumberFormatRegexes.TryGetValueWithDefault(countryPart, new List<Regex>());
            if (regexes.Count == 0) {
                regexes = VatNumberFormatRegexes.TryGetValueWithDefault(countryPart, new List<Regex>());
            }
            var match = false;
            foreach (var regex in regexes) {
                var matchResult = regex.Match(vatNumber);
                if (!matchResult.Success) {
                    continue;
                }
                match = true;
            }
            return match;
        }

        public bool HasValidFormatCheck(VatNumber entity) {
            var countryPart = entity.CleanedCountryCode;
            var checkFunction = EuropeanVatNumberCheckFunctions.TryGetValueWithDefault(countryPart);
            return checkFunction == null || checkFunction(entity);
        }

        private static Dictionary<string, List<Regex>> FillEuropeanFormatRegexes() {
            // Formats retrieved from: http://ec.europa.eu/taxation_customs/vies/faq.html
            // Regexes built using http://regexr.com
            var result = new Dictionary<string, List<Regex>> {
                { "AT", new List<Regex> {CreateFromPattern("(ATU)[0-9]{8}")}},
                { "BE", new List<Regex> {CreateFromPattern("(BE[0-1]{1})[0-9]{9}")}},
                { "BG", new List<Regex> {CreateFromPattern("(BG)[0-9]{9,10}")}},
                { "CY", new List<Regex> {CreateFromPattern("(CY)[0-9]{8}[A-Z]")}},
                { "CZ", new List<Regex> {CreateFromPattern("(CZ)[0-9]{8,10}")}},
                { "DE", new List<Regex> {CreateFromPattern("(DE)[0-9]{9}")}},
                { "DK", new List<Regex> {CreateFromPattern("(DK)([0-9]{2} ?){3}[0-9]{2}")}},
                { "EE", new List<Regex> {CreateFromPattern("(EE)[0-9]{9}")}},
                { "EL", new List<Regex> {CreateFromPattern("(EL)[0-9]{9}")}},
                { "ES", new List<Regex> {CreateFromPattern("(ES)[A-Z0-9]{1}[0-9]{7}[A-Z0-9]{1}")}},
                { "FI", new List<Regex> {CreateFromPattern("(FI)[0-9]{8}")}},
                { "FR", new List<Regex> {CreateFromPattern("(FR)[A-Z0-9]{2}[0-9]{9}")}},
                { "HR", new List<Regex> {CreateFromPattern("(HR)[0-9]{11}")}},
                { "HU", new List<Regex> {CreateFromPattern("(HU)[0-9]{8}")}},
                { "IE", new List<Regex> {CreateFromPattern("(IE)[0-9]{1}[A-Z0-9+*]{1}[0-9]{5}[A-Z]{1}"), CreateFromPattern("(IE)[0-9]{7}(WI)")}},
                { "IT", new List<Regex> {CreateFromPattern("(IT)[0-9]{11}")}},
                { "LT", new List<Regex> {CreateFromPattern("(LT)([0-9]{3}){3,4}")}},
                { "LU", new List<Regex> {CreateFromPattern("(LU)[0-9]{8}")}},
                { "LV", new List<Regex> {CreateFromPattern("(LV)[0-9]{11}")}},
                { "MT", new List<Regex> {CreateFromPattern("(MT)[0-9]{8}")}},
                { "NL", new List<Regex> {CreateFromPattern("(NL)[0-9]{9}(B)[0-9]{2}")}},
                { "PL", new List<Regex> {CreateFromPattern("(PL)[0-9]{10}")}},
                { "PT", new List<Regex> {CreateFromPattern("(PT)[0-9]{9}")}},
                { "RO", new List<Regex> {CreateFromPattern("(RO)[0-9]{2,9}")}},
                { "SE", new List<Regex> {CreateFromPattern("(SE)[0-9]{12}")}},
                { "SI", new List<Regex> {CreateFromPattern("(SI)[0-9]{8}")}},
                { "SK", new List<Regex> {CreateFromPattern("(SK)[0-9]{10}")}}
            };
            return result;
        }

        private static Dictionary<string, List<Regex>> FillFormatRegexes() {
            // Formats retrieved from: https://en.wikipedia.org/wiki/VAT_identification_number
            // Regexes built using http://regexr.com
            return new Dictionary<string, List<Regex>> {
                // Non-European
                { "AL", new List<Regex> { CreateFromPattern("(AL)[J,K][0-9]{8}[A-Z]") }},
                { "AU", new List<Regex> { CreateFromPattern("(AU)[0-9]{11}") }},
                { "BY", new List<Regex> { CreateFromPattern("(BY)[0-9]{9}") }},
                { "CA", new List<Regex> { CreateFromPattern("(CA)[0-9]{9}[A-Z]{2}[0-9]{4}") }},
                { "CH", new List<Regex> { CreateFromPattern("(CHE)[0-9]{6}"), CreateFromPattern("(CHE)[0-9]{9}((TVA)|(MWST)|(IVA))") }},
                { "GB", new List<Regex> {CreateFromPattern("(GB)[0-9]{3}( )?[0-9]{4}( )?[0-9]{2}(( )?[0-9]{3})?"), CreateFromPattern("(GB(GD|HA))[0-9]{3}")}},
                { "IS", new List<Regex> { CreateFromPattern("(IS)[0-9]{5,7}") }},
                { "IN", new List<Regex> { CreateFromPattern("(IN)[0-9]{11}[VC]{1}") }},
                { "IL", new List<Regex> { CreateFromPattern("(IL)[0-9]{9}") }},
                { "NZ", new List<Regex> { CreateFromPattern("(NZ)[0-9]{13}") }},
                { "NO", new List<Regex> { CreateFromPattern("(NO)[0-9]{9}") }},
                { "PH", new List<Regex> { CreateFromPattern("(PH)([0-9]{3}( )?){3}[0-9]{3}") }},
                { "RU", new List<Regex> { CreateFromPattern("(RU)([0-9]{10})([0-9]{2})?") }},
                { "SM", new List<Regex> { CreateFromPattern("(SM)[0-9]{5}") }},
                { "RS", new List<Regex> { CreateFromPattern("(RS)[0-9]{9}") }},
                { "TR", new List<Regex> { CreateFromPattern("(TR)[0-9]{10}") }},
                { "UA", new List<Regex> { CreateFromPattern("(UA)[0-9]{10}") }},
                { "UZ", new List<Regex> { CreateFromPattern("(UZ)[0-9]{9}") }},
                // Latin-American
                { "AR", new List<Regex> { CreateFromPattern("(AR)[0-9]{11}") }},
                { "BO", new List<Regex> { CreateFromPattern("(BO)[0-9]{7}") }},
                //{ "BR", new List<Regex> { CreateFromPattern("(BR)[0-9]{14}") }}
                { "CL", new List<Regex> { CreateFromPattern("(CL)[0-9]{8}(-)[0-9]{1}") }},
                { "CO", new List<Regex> { CreateFromPattern("(CO)[0-9]{10}") }},
                { "EC", new List<Regex> { CreateFromPattern("(EC)[0-9]{13}") }},
                { "GT", new List<Regex> { CreateFromPattern("(GT)[0-9]{7}(-)[0-9]{1}") }}
            };
        }

        private static Dictionary<string, Func<VatNumber, bool>> FillCheckFunctions() {
            var result = new Dictionary<string, Func<VatNumber, bool>>();
            result.Add("AT", vatNumber => {
                var c = new[] { 1, 2, 1, 2, 1, 2, 1 };
                var total = 0;
                var vat = vatNumber.VatNumberWithoutCountryCode.Substring(1);
                for (var index = 0; index < 7; index++) {
                    var d = ConvertToInt(vat[index]) * c[index];
                    if (d > 9) {
                        d = (int) (Math.Floor(d / 10.0) + d % 10);
                    }
                    total += d;
                }
                total = 10 - (total + 4) % 10;
                if (total == 10) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(7, 1));
            });
            result.Add("BE", vatNumber => {
                var vat = ConvertToInt(vatNumber.VatNumberWithoutCountryCode.Substring(0, 8));
                var check = ConvertToInt(vatNumber.VatNumberWithoutCountryCode.Substring(8, 2));
                return (vat + check) % 97 == 0;
            });
            result.Add("BG", vatNumber => {
                var total = 0;
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (vat.Length == 9) {
                    for (var index = 0; index < 8; index++) {
                        total += ConvertToInt(vat[index]) * (index + 1);
                    }
                    total %= 11;
                    if (total == 10) {
                        total = 0;
                        for (var index = 0; index < 8; index++) {
                            total += ConvertToInt(vat[index]) * (index + 3);
                        }
                        total %= 11;
                    }
                    return total % 10 == ConvertToInt(vat.Substring(8));
                }
                if (vat.Length == 10) {
                    var month = ConvertToInt(vat.Substring(2, 2));
                    var moddedMonth = month % 20;
                    if (Enum.IsDefined(typeof(Month), moddedMonth)) {
                        var multipliers = new[] { 2, 4, 8, 5, 10, 9, 7, 3, 6 };
                        total = 0;
                        for (var index = 0; index < 9; index++) {
                            total += ConvertToInt(vat[index]) * multipliers[index];
                        }
                        total %= 11;
                        total %= 10;
                        if (total == ConvertToInt(vat.Substring(9, 1))) {
                            return true;
                        }
                    }
                    var multipliersExtra = new[] { 21, 19, 17, 13, 11, 9, 7, 3, 1 };
                    total = 0;
                    for (var index = 0; index < 9; index++) {
                        total += ConvertToInt(vat[index]) * multipliersExtra[index];
                    }
                    if (total % 10 == ConvertToInt(vat.Substring(9, 1))) {
                        return true;
                    }
                    multipliersExtra = new[] { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
                    total = 0;
                    for (var index = 0; index < 9; index++) {
                        total += ConvertToInt(vat[index]) * multipliersExtra[index];
                    }
                    total = 11 - (total % 11);
                    if (total == 10) {
                        return false;
                    }
                    if (total == 11) {
                        total = 0;
                    }
                    return total == ConvertToInt(vat.Substring(9, 1));
                }
                return false;
            });
            result.Add("CY", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (ConvertToInt(vat.Substring(0, 2)) == 12) {
                    return false;
                }
                var check = new Dictionary<int, int> {
                    {0, 1 }, {1, 0}, {2, 5}, {3, 7}, {4, 9}, {5, 13}, {6, 15}, {7, 17}, {8, 19}, {9, 21}
                };
                var b = 0;
                for (var index = 0; index < 8; index++) {
                    var temp = ConvertToInt(vat[index]);
                    if (index % 2 == 0) {
                        temp = check[temp];
                    }
                    b += temp;
                }
                const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var letter = alphabet[b % 26];
                return letter.ToString() == vat.Substring(8, 1);
            });
            result.Add("CZ", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 8, 7, 6, 5, 4, 3, 2 };
                var regexes = new List<Regex> {
                    CreateFromPattern("\\d{8}"),
                    CreateFromPattern("[0-5][0-9][0|1|5|6]\\d[0-3]\\d\\d{3}"),
                    CreateFromPattern("6\\d{8}"),
                    CreateFromPattern("\\d{2}[0-3|5-8]\\d[0-3]\\d\\d{4}")
                };
                if (regexes[0].IsMatch(vat)) {
                    for (var i = 0; i < 7; i++) {
                        total += ConvertToInt(vat[i]) * multipliers[i];
                    }
                    total = 11 - (total % 11);
                    if (total == 10) {
                        total = 0;
                    }
                    if (total == 11) {
                        total = 1;
                    }
                    return total == ConvertToInt(vat.Substring(7, 1));
                }
                if (regexes[1].IsMatch(vat)) {
                    return ConvertToInt(vat.Substring(0, 2)) <= 53;
                }
                if (regexes[2].IsMatch(vat)) {
                    for (var i = 0; i < 7; i++) {
                        total += ConvertToInt(vat[i + 1]) * multipliers[i];
                    }
                    total = 11 - (total % 11);
                    if (total == 10) {
                        total = 0;
                    }
                    if (total == 11) {
                        total = 1;
                    }
                    var lookup = new[] { 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 10 };
                    return lookup[total - 1] == ConvertToInt(vat.Substring(8, 1));
                }
                if (regexes[3].IsMatch(vat)) {
                    var temp = ConvertToInt(vat.Substring(0, 2)) + ConvertToInt(vat.Substring(2, 2))
                               + ConvertToInt(vat.Substring(4, 2)) + ConvertToInt(vat.Substring(6, 2));
                    return temp % 11 == 0 && ConvertToInt(vat.ToString()) % 11 == 0;
                }
                return false;
            });
            result.Add("DE", vatNumber => {
                var product = 10;
                var vat = vatNumber.VatNumberWithoutCountryCode;
                for (var i = 0; i < 8; i++) {
                    var sum = (ConvertToInt(vat[i]) + product) % 10;
                    if (sum == 0) {
                        sum = 10;
                    }
                    product = (2 * sum) % 11;
                }
                var checkDigit = 11 - product;
                if (checkDigit == 10) {
                    checkDigit = 0;
                }
                return checkDigit == ConvertToInt(vat.Substring(8, 1));
            });
            result.Add("DK", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 2, 7, 6, 5, 4, 3, 2, 1 };
                for (var i = 0; i < 8; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                return total % 11 == 0;
            });
            result.Add("EE", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 3, 7, 1, 3, 7, 1, 3, 7 };
                for (var i = 0; i < 8; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 10 - (total % 10);
                if (total == 10) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(8, 1));
            });
            result.Add("EL", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (vat.Length == 8) {
                    vat = "0" + vat;
                }
                var total = 0;
                var multipliers = new[] { 256, 128, 64, 32, 16, 8, 4, 2 };
                for (var i = 0; i < 8; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total %= 11;
                if (total > 9) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(8, 1));
            });
            result.Add("ES", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                int temp;
                var multipliers = new[] { 2, 1, 2, 1, 2, 1, 2 };
                var regexes = new List<Regex> {
                    CreateFromPattern("[A-H|J|U|V]\\d{8}"),
                    CreateFromPattern("[A-H|N-S|W]\\d{7}[A-J]"),
                    CreateFromPattern("[0-9|Y|Z]\\d{7}[A-Z]"),
                    CreateFromPattern("[K|L|M|X]\\d{7}[A-Z]")
                };
                if (regexes[0].IsMatch(vat)) {
                    for (var i = 0; i < 7; i++) {
                        temp = ConvertToInt(vat[i + 1]) * multipliers[i];
                        if (temp > 9) {
                            total += (int) (Math.Floor(temp / 10.0) + temp % 10);
                        } else {
                            total += temp;
                        }
                    }
                    total = 10 - (total % 10);
                    if (total == 10) {
                        total = 0;
                    }
                    return total == ConvertToInt(vat.Substring(8, 1));
                }
                if (regexes[1].IsMatch(vat)) {
                    for (var i = 0; i < 7; i++) {
                        temp = ConvertToInt(vat[i + 1]) * multipliers[i];
                        if (temp > 9) {
                            total += (int) (Math.Floor(temp / 10.0) + temp % 10);
                        } else {
                            total += temp;
                        }
                    }
                    total = 10 - (total % 10);
                    return (char) (total + 64) == vat[8];
                }
                var letters = "TRWAGMYFPDXBNJZSQVHLCKE";
                if (regexes[2].IsMatch(vat)) {
                    var tempVat = vat;
                    if (tempVat[0] == 'Y') {
                        tempVat = tempVat.Replace("Y", "1");
                    }
                    if (tempVat[0] == 'Z') {
                        tempVat = tempVat.Replace("Z", "1");
                    }
                    return tempVat[8] == letters[ConvertToInt(tempVat.Substring(0, 8)) % 23];
                }
                if (regexes[3].IsMatch(vat)) {
                    return vat[8] == letters[ConvertToInt(vat.Substring(1, 8)) % 23];
                }
                return false;
            });
            result.Add("FI", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 7, 9, 10, 5, 8, 4, 2 };
                for (var i = 0; i < 7; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 11 - (total % 11);
                if (total > 9) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(7, 1));
            });
            result.Add("FR", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var siren = ConvertToInt(vat.Substring(2, 9));
                var key = (12 + 3 * (siren % 97)) % 97;
                return key == ConvertToInt(vat.Substring(0, 2));
            });
            result.Add("GB", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (vat.Substring(0, 2) == "GD") {
                    return ConvertToInt(vat.Substring(2, 3)) < 500;
                }
                if (vat.Substring(0, 2) == "HA") {
                    return ConvertToInt(vat.Substring(2, 3)) > 499;
                }
                if (ConvertToInt(vat) == 0) {
                    return false;
                }
                var multipliers = new[] { 8, 7, 6, 5, 4, 3, 2 };
                var total = 0;
                var no = ConvertToInt(vat.Substring(0, 7));
                for (var i = 0; i < 7; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                var checkDigit = total;
                while (checkDigit > 0) {
                    checkDigit = checkDigit - 97;
                }
                checkDigit = Math.Abs(checkDigit);
                if (checkDigit == ConvertToInt(vat.Substring(7, 2))
                    && no < 9990001
                    && (no < 100000 || no > 999999)
                    && (no < 9490001 || no > 9700000)) {
                    return true;
                }
                if (checkDigit >= 55) {
                    checkDigit -= 55;
                } else {
                    checkDigit = checkDigit + 42;
                }
                return (checkDigit == ConvertToInt(vat.Substring(7, 2)) && no > 1000000);
            });
            result.Add("HR", vatNumber => {
                var product = 10;
                var vat = vatNumber.VatNumberWithoutCountryCode;
                for (var i = 0; i < 10; i++) {
                    var sum = (ConvertToInt(vat[i]) + product) % 10;
                    if (sum == 0) {
                        sum = 10;
                    }
                    product = (2 * sum) % 11;
                }
                var checkDigit = (product + ConvertToInt(vat.Substring(10, 1))) % 10;
                return checkDigit == 1;
            });
            result.Add("HU", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 9, 7, 3, 1, 9, 7, 3 };
                for (var i = 0; i < 7; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 10 - (total % 10);
                if (total == 10) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(7, 1));
            });
            result.Add("IE", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 8, 7, 6, 5, 4, 3, 2 };
                if (new Regex("\\d[A-Z\\*\\+]").IsMatch(vat)) {
                    vat = "0" + vat.Substring(2, 5) + vat.Substring(0, 1) + vat.Substring(7, 1);
                }
                for (var i = 0; i < 7; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total %= 23;
                if (total == 0) {
                    total = 'W';
                } else {
                    total = (char) (total + 64);
                }
                return total == vat[7];
            });
            result.Add("IT", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var multipliers = new[] { 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 };
                if (ConvertToInt(vat.Substring(0, 7)) == 0) {
                    return false;
                }
                var temp = ConvertToInt(vat.Substring(7, 3));
                if ((temp < 0) || (temp > 201) && temp != 999 && temp != 888) {
                    return false;
                }
                var total = 0;
                for (var i = 0; i < 10; i++) {
                    temp = ConvertToInt(vat[i]) * multipliers[i];
                    if (temp > 9) {
                        total += (int) (Math.Floor(temp / 10.0) + temp % 10);
                    } else {
                        total += temp;
                    }
                }
                total = 10 - (total % 10);
                if (total > 9) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(10, 1));
            });
            result.Add("LT", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (vat.Length == 9) {
                    if (!new Regex("\\d{7}1").IsMatch(vat)) {
                        return false;
                    }
                    var total = 0;
                    for (var i = 0; i < 8; i++) {
                        total += ConvertToInt(vat[i]) * (i + 1);
                    }
                    if (total % 11 == 10) {
                        var multipliers = new[] { 3, 4, 5, 6, 7, 8, 9, 1 };
                        total = 0;
                        for (var i = 0; i < 8; i++) {
                            total += ConvertToInt(vat[i]) * multipliers[i];
                        }
                    }
                    total %= 11;
                    if (total == 10) {
                        total = 0;
                    }
                    return total == ConvertToInt(vat.Substring(8, 1));
                } else {
                    if (!new Regex("\\d{10}1").IsMatch(vat)) {
                        return false;
                    }
                    var total = 0;
                    var multipliers = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1, 2 };
                    for (var i = 0; i < 11; i++) {
                        total += ConvertToInt(vat[i]) * multipliers[i];
                    }
                    if (total % 11 == 10) {
                        var multipl = new[] { 3, 4, 5, 6, 7, 8, 9, 1, 2, 3, 4 };
                        total = 0;
                        for (var i = 0; i < 11; i++) {
                            total += ConvertToInt(vat[i]) * multipl[i];
                        }
                    }
                    total %= 11;
                    if (total == 10) {
                        total = 0;
                    }
                    return total == ConvertToInt(vat.Substring(11, 1));
                }
            });
            result.Add("LU", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                return ConvertToInt(vat.Substring(0, 6)) % 89 == ConvertToInt(vat.Substring(6, 2));
            });
            result.Add("LV", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                if (new Regex("/^[0-3]/").IsMatch(vat)) {
                    return new Regex("/^[0-3][0-9][0-1][0-9]/").IsMatch(vat);
                }
                var total = 0;
                var multipliers = new[] { 9, 1, 4, 8, 3, 10, 2, 5, 7, 6 };
                for (var i = 0; i < 10; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                if (total % 11 == 4 && vat[0] == '9') {
                    total -= 45;
                }
                if (total % 11 == 4) {
                    total = 4 - (total % 11);
                } else if (total % 11 > 4) {
                    total = 14 - (total % 11);
                } else if (total % 11 < 4) {
                    total = 3 - (total % 11);
                }
                return total == ConvertToInt(vat.Substring(10, 1));
            });
            result.Add("MT", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 3, 4, 6, 7, 8, 9 };
                for (var i = 0; i < 6; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 37 - (total % 37);
                return total == ConvertToInt(vat.Substring(6, 2));
            });
            //result.Add("NL", vatNumber => {
            //    var vat = vatNumber.VatNumberWithoutCountryCode;
            //    var total = 0;
            //    var multipliers = new[] { 9, 8, 7, 6, 5, 4, 3, 2 };
            //    for (var i = 0; i < 8; i++) {
            //        total += ConvertToInt(vat[i]) * multipliers[i];
            //    }
            //    total %= 11;
            //    if (total > 9) {
            //        total = 0;
            //    }
            //    return total == ConvertToInt(vat.Substring(8, 1));
            //});
            result.Add("PL", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 6, 5, 7, 2, 3, 4, 5, 6, 7 };
                for (var i = 0; i < 9; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total %= 11;
                if (total > 9) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(9, 1));
            });
            result.Add("PT", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 9, 8, 7, 6, 5, 4, 3, 2 };
                for (var i = 0; i < 8; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 11 - (total % 11);
                if (total > 9) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(8, 1));
            });
            result.Add("RO", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var multipliers = new[] { 7, 5, 3, 2, 1, 7, 5, 3, 2 };
                var vatLength = vat.Length - 1;
                var temp = new int[vatLength];
                Array.Copy(multipliers, 9 - vatLength, temp, 0, vatLength);
                multipliers = temp;
                var total = 0;
                for (var i = 0; i < vat.Length - 1; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = (10 * total) % 11;
                if (total == 10) {
                    total = 0;
                }
                return total == ConvertToInt(vat.Substring(vat.Length - 1, 1));
            });
            result.Add("SE", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var r = 0;
                for (var i = 0; i < 9; i += 2) {
                    var digit = ConvertToInt(vat[i]);
                    r += ((int) Math.Floor(digit / 5.0)) + ((digit * 2) % 10);
                }
                var s = 0;
                for (var i = 1; i < 9; i += 2) {
                    s += ConvertToInt(vat[i]);
                }
                var checkDigit = (10 - (r + s) % 10) % 10;
                return checkDigit == ConvertToInt(vat.Substring(9, 1));
            });
            result.Add("SI", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                var total = 0;
                var multipliers = new[] { 8, 7, 6, 5, 4, 3, 2 };
                for (var i = 0; i < 7; i++) {
                    total += ConvertToInt(vat[i]) * multipliers[i];
                }
                total = 11 - (total % 11);
                if (total == 10) {
                    total = 0;
                }
                return total != 11 && total == ConvertToInt(vat.Substring(7, 1));
            });
            result.Add("SK", vatNumber => {
                var vat = vatNumber.VatNumberWithoutCountryCode;
                return vat.To<long>() % 11 == 0;
            });
            return result;
        }

        private static Regex CreateFromPattern(string pattern) {
            // Added ^ and $ to match exact pattern
            return new Regex("^" + pattern + "$", RegexOptions.Compiled);
        }

        /// <summary>
        /// Different conversion-method for converting a string to an int.
        /// The default converter will convert the character to it's ASCII int-value
        /// so the character '1' becomes 49.
        /// </summary>
        /// <param name="character">The character value</param>
        /// <returns>The integer value</returns>
        private static int ConvertToInt(char character) {
            return character.ToString().To<int>();
        }

        /// <summary>
        /// Different conversion-method for converting a string to an int.
        /// The default converter will convert the character to it's ASCII int-value
        /// so the character '1' becomes 49.
        /// </summary>
        /// <param name="value">The character value</param>
        /// <returns>The integer value</returns>
        private static int ConvertToInt(string value) {
            if (value.Length != 1) { return value.To<int>(); }

            var character = value[0];
            return ConvertToInt(character);
        }
    }
}
