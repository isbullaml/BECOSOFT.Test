using BECOSOFT.Data.Models.Common;
using BECOSOFT.Data.Validation;
using BECOSOFT.Data.Validation.Common;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace BECOSOFT.Data.Extensions.Communication {
    public static class MailHelper {
        private static readonly EmailValidator Validator = new EmailValidator();

        /// <summary>
        /// Validates the provided <paramref name="email"/> with the <see cref="EmailValidator"/>.
        /// The <paramref name="email"/> is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">email address(-es) to validate. A ';' character as separator is allowed.</param>
        /// <param name="mayContainMultiple">Indicates that the <paramref name="email"/> cannot contain multiple email addresses</param>
        /// <returns>Returns whether the provided <see cref="email"/> value (containing one or more email addresses, separated by ';') are valid.</returns>
        public static bool IsValidEmailAddress(string email, bool mayContainMultiple = true) {
            var validationResult = GetEmailValidationResults(email);
            if (validationResult.Results.IsEmpty()) { return false; }
            if (!mayContainMultiple && validationResult.Results.Count > 1) {
                return false;
            }
            return validationResult.IsValid();
        }

        /// <summary>
        /// Returns the <see cref="MailAddress"/> objects extracted from the provided <paramref name="email"/>
        /// The <paramref name="email"/> is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">email address(-es) to validate. A ';' character as separator is allowed.</param>
        /// <returns>Returns the <see cref="MailAddress"/> objects extracted from the provided <paramref name="email"/></returns>
        public static List<MailAddress> GetMailAddresses(string email) {
            if (email.IsNullOrWhiteSpace()) { return new List<MailAddress>(0); }
            var split = email.ToSplitList<string>(';');
            if (split.IsEmpty()) { return new List<MailAddress>(0); }
            var result = new List<MailAddress>();
            var emailEntities = split.Select(s => new Email(s)).ToList();
            var validationResult = Validator.Validate(emailEntities);
            foreach (var validResult in validationResult.ValidResults) {
                var emailEntity = validResult.ValidatedEntity;
                result.Add(new MailAddress(emailEntity.Address));
            }
            return result;
        }

        /// <summary>
        /// Validates the provided <paramref name="email"/> with the <see cref="EmailValidator"/>.
        /// If the <see cref="Email"/>.<see cref="Email.Address"/> property contains a ';', it is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">Email address to validate. A ';' character as separator is allowed.</param>
        /// <returns>Returns whether the provided <see cref="email"/> value (containing one or more email addresses, separated by ';') are valid.</returns>
        public static bool IsValidEmailAddress(Email email) {
            var validationResult = GetEmailValidationResults(email);
            return validationResult.IsValid();
        }

        /// <summary>
        /// Validates te provided <paramref name="email"/> and returns the validation result
        /// If the <see cref="Email"/>.<see cref="Email.Address"/> property contains a ';', it is split by the ';' character and each email element is validated.
        /// The <paramref name="email"/> is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">Email address to validate. A ';' character as separator is allowed.</param>
        /// <returns>Returns the <see cref="MailAddress"/> objects extracted from the provided <paramref name="email"/></returns>
        public static MultiValidationResult<Email> GetEmailValidationResults(Email email) {
            List<Email> emailEntities;
            if (email.Address != null && email.Address.Contains(';')) {
                var split = email.Address.ToSplitList<string>(';');
                emailEntities = split.Select(s => new Email(s)).ToList();
            } else {
                emailEntities = new List<Email>(1) { email };
            }
            var validationResult = Validator.Validate(emailEntities);
            return validationResult;
        }

        /// <summary>
        /// Validates te provided <paramref name="email"/> and returns the validation result
        /// If the <see cref="Email"/>.<see cref="Email.Address"/> property contains a ';', it is split by the ';' character and each email element is validated.
        /// The <paramref name="email"/> is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">Email address to validate. A ';' character as separator is allowed.</param>
        /// <returns>Returns the <see cref="MailAddress"/> objects extracted from the provided <paramref name="email"/></returns>
        public static MultiValidationResult<Email> GetEmailValidationResults(string email) {
            if (email.IsNullOrWhiteSpace()) { return MultiValidationResult<Email>.Empty(); }
            var split = email.ToSplitList<string>(';');
            if (split.IsEmpty()) { return MultiValidationResult<Email>.Empty(); }
            var emailEntities = split.Select(s => new Email(s)).ToList();
            var validationResult = Validator.Validate(emailEntities);
            return validationResult;
        }

        /// <summary>
        /// Validates te provided <paramref name="email"/> and returns the validation result
        /// If the <see cref="Email"/>.<see cref="Email.Address"/> property contains a ';', it is split by the ';' character and each email element is validated.
        /// The <paramref name="email"/> is split by the ';' character and each email element is validated.
        /// </summary>
        /// <param name="email">Email address to validate. A ';' character as separator is allowed.</param>
        /// <returns>Returns the <see cref="MailAddress"/> objects extracted from the provided <paramref name="email"/></returns>
        public static List<MailAddress> GetMailAddresses(Email email) {
            List<Email> emailEntities;
            if (email.Address != null && email.Address.Contains(';')) {
                var split = email.Address.ToSplitList<string>(';');
                emailEntities = split.Select(s => new Email(s)).ToList();
            } else {
                emailEntities = new List<Email>(1) { email };
            }
            var validationResult = Validator.Validate(emailEntities);
            var result = new List<MailAddress>();
            foreach (var validResult in validationResult.ValidResults) {
                var emailEntity = validResult.ValidatedEntity;
                result.Add(new MailAddress(emailEntity.Address));
            }
            return result;
        }
    }
}