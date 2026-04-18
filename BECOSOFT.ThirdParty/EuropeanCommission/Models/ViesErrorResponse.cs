using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Models {
    public static class ViesErrorResponse {
        /// <summary>
        /// INVALID_INPUT: The provided CountryCode is invalid or the VAT number is empty
        /// </summary>
        public static ViesError InvalidInput = new ViesError("INVALID_INPUT", "The provided CountryCode is invalid or the VAT number is empty");

        /// <summary>
        /// GLOBAL_MAX_CONCURRENT_REQ: Your Request for VAT validation has not been processed; the maximum number of concurrent requests has been reached.
        /// Please re-submit your request later or contact TAXUD-VIESWEB@ec.europa.eu for further information":
        /// Your request cannot be processed due to high traffic on the web application. Please try again later.
        /// </summary>
        public static ViesError GlobalMaxConcurrentRequests = new ViesError("GLOBAL_MAX_CONCURRENT_REQ", "Your Request for VAT validation has not been processed; the maximum number of concurrent requests has been reached.");

        /// <summary>
        /// MS_MAX_CONCURRENT_REQ: Your Request for VAT validation has not been processed; the maximum number of concurrent requests for this Member State has been reached.
        /// Please re-submit your request later or contact TAXUD-VIESWEB@ec.europa.eu for further information":
        /// Your request cannot be processed due to high traffic towards the Member State you are trying to reach. Please try again later.
        /// </summary>
        public static ViesError MemberStateMaxConcurrentRequests = new ViesError("MS_MAX_CONCURRENT_REQ", "Your Request for VAT validation has not been processed; the maximum number of concurrent requests for this Member State has been reached.");

        /// <summary>
        /// SERVICE_UNAVAILABLE: An error was encountered either at the network level or the Web application level, try again later.
        /// </summary>
        public static ViesError ServiceUnavailable = new ViesError("SERVICE_UNAVAILABLE", "An error was encountered either at the network level or the Web application level, try again later.");

        /// <summary>
        /// MS_UNAVAILABLE: The application at the Member State is not replying or not available. Please refer to the Technical Information page to check the status of the requested Member State, try again later
        /// </summary>
        public static ViesError MemberStateUnavailable = new ViesError("MS_UNAVAILABLE", "The application at the Member State is not replying or not available. Please refer to the Technical Information page to check the status of the requested Member State, try again later");

        /// <summary>
        /// TIMEOUT: The application did not receive a reply within the allocated time period, try again later.
        /// </summary>
        public static ViesError Timeout = new ViesError("TIMEOUT", "The application did not receive a reply within the allocated time period, try again later.");

        public static List<ViesError> ViesErrors = new List<ViesError> {
            InvalidInput,
            GlobalMaxConcurrentRequests,
            MemberStateMaxConcurrentRequests,
            ServiceUnavailable,
            MemberStateUnavailable,
            Timeout,
        };

        public static Dictionary<string, ViesError> ViesErrorMapping = ViesErrors.ToDictionary(e => e.Error, e => e);

        public static HashSet<string> Errors = new HashSet<string>(ViesErrorMapping.Keys);

        public static HashSet<string> RetryableErrors = new HashSet<string> {
            GlobalMaxConcurrentRequests.Error,
            MemberStateMaxConcurrentRequests.Error,
            Timeout.Error
        };

        public static string GetErrorMessage(string error) {
            foreach (var viesError in ViesErrors) {
                if (error.EqualsIgnoreCase(viesError.Error)) {
                    return viesError.Message;
                }
            }
            return "Unknown error";
        }
    }

    public class ViesError {
        public string Error { get; }
        public string Message { get; }

        internal ViesError(string error, string message) {
            Error = error;
            Message = message;
        }
    }
}