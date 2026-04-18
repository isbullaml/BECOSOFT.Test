using System;
using System.Net.Mail;
using BECOSOFT.Data.Models.Common;
using BECOSOFT.Utilities.Extensions;
using NLog;
using System.Globalization;

namespace BECOSOFT.Data.Validation.Common {
    public sealed class EmailValidator : Validator<Email> {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private const int EmailMaxLength = 100;

        public EmailValidator() : base(Logger, null) {
        }

        protected override void ExtraValidation(IValidationContainer<Email> container) {
            var entities = container.Entities;
            var errors = container.ErrorList;
            const string addressPropery = nameof(Email.Address);
            for (var i = 0; i < entities.Count; i++) {
                var entity = entities[i];
                var errorList = errors[i];
                var email = entity.Address?.Trim();
                var displayName = entity.DisplayName?.Trim().NullIf("");
                if (email.IsNullOrWhiteSpace()) {
                    errorList.Add(addressPropery, Resources.Email_Validation_Empty);
                } else {
                    var isMalformed = false;
                    try {
                        if (email.Contains("@@") || email.Contains("..")) {
                            isMalformed = true;
                            continue;
                        }
                        var atIndex = email.IndexOf("@", StringComparison.InvariantCultureIgnoreCase);
                        if (atIndex < 0) {
                            isMalformed = true;
                            continue;
                        } 
                        if (atIndex == email.Length - 1) {
                            isMalformed = true;
                            continue;
                        }
                        var partAfterAt = email.Substring(atIndex + 1, email.Length - (atIndex + 1));
                        if (partAfterAt.IsNullOrWhiteSpace()) {
                            isMalformed = true;
                            continue;
                        }
                        var lastDotIndex = partAfterAt.LastIndexOf('.');
                        if (lastDotIndex < 0) {
                            isMalformed = true;
                            continue;
                        }
                        if ((partAfterAt.Length - 1) - lastDotIndex < 2) {
                            isMalformed = true;
                            continue;
                        }
                        if (email.LastIndexOf("@", StringComparison.InvariantCultureIgnoreCase) != atIndex) {
                            isMalformed = true;
                            continue;
                        } 
                        if (email.Length > EmailMaxLength) {
                            errorList.Add(addressPropery, string.Format(Resources.Email_Validation_Length, EmailMaxLength));
                            continue;
                        } 
                        if (!email.Equals(email.RemoveControlCharacters())) {
                            isMalformed = true;
                            continue;
                        }
                        
                        var lessThanIndex = email.IndexOf("<", StringComparison.InvariantCultureIgnoreCase);
                        var greaterThanIndex = email.IndexOf(">", StringComparison.InvariantCultureIgnoreCase);
                        if (lessThanIndex >= 0 && greaterThanIndex >= 0) {
                            if (greaterThanIndex < lessThanIndex) {
                                isMalformed = true;
                                continue;
                            }
                            if (atIndex < lessThanIndex || atIndex > greaterThanIndex || lessThanIndex == greaterThanIndex + 1) {
                                isMalformed = true;
                                continue;
                            }
                            var partBetween = email.Substring(lessThanIndex + 1, (greaterThanIndex - lessThanIndex) - 1);
                            var partBetweenCleaned = partBetween.RemoveControlCharacters().RemoveWhitespace();
                            if (partBetween.Contains(UnicodeCategory.SpaceSeparator) || partBetween != partBetweenCleaned) {
                                isMalformed = true;
                                continue;
                            }
                            
                        } else if ((lessThanIndex < 0 && greaterThanIndex >= 0) || (lessThanIndex >= 0 && greaterThanIndex < 0)) {
                            isMalformed = true;
                            continue;
                        } else if (email.Contains(UnicodeCategory.SpaceSeparator)) {
                            isMalformed = true;
                            continue;
                        }
                        try {
                            _ = new MailAddress(email, displayName);
                        } catch (FormatException) {
                            isMalformed = true;
                        }
                    } finally {
                        if (isMalformed) {
                            errorList.Add(addressPropery, Resources.Email_Validation_Malformed);
                        }
                    }
                }
            }
        }
    }
}