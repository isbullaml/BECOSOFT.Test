using BECOSOFT.ThirdParty.EuropeanCommission.Models;
using BECOSOFT.ThirdParty.EuropeanCommission.Services.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BECOSOFT.ThirdParty.EuropeanCommission.Services {
    internal class EoriValidationService : IEoriValidationService {
        private static readonly Uri EoriUri = new Uri("https://ec.europa.eu/taxation_customs/dds2/eos/validation/services/validation");
        private static readonly int NumberOfAttempts = 3;

        public EoriValidationResponse Validate(EoriNumber eoriNumber) {
            var validationResult = Task.Run(() => ValidateAsync(eoriNumber));
            return validationResult.Result;
        }

        public async Task<EoriValidationResponse> ValidateAsync(EoriNumber eoriNumber) {
            EoriValidationResponse response = null;
            for (var i = 0; i < NumberOfAttempts; i++) {
                response = await PerformRequest(eoriNumber);
                if (response.Error == null) {
                    break;
                }

                await Task.Delay(500);
            }
            return response;
        }

        private async Task<EoriValidationResponse> PerformRequest(EoriNumber vatNumber) {
            try {
                var request = CreateRequest();
                using (var requestStream = await request.GetRequestStreamAsync()) {
                    var requestData = GetRequestData(vatNumber);
                    await requestStream.WriteAsync(requestData, 0, requestData.Length);
                    using (var response = (HttpWebResponse)await request.GetResponseAsync()) {
                        using (var streamReader = new StreamReader(response.GetResponseStream())) {
                            var responseString = await streamReader.ReadToEndAsync();
                            var parsedResponse = ParseResponse(responseString);
                            return parsedResponse;
                        }
                    }
                }
            } catch (Exception e) {
                return new EoriValidationResponse(e);
            }
        }

        private static HttpWebRequest CreateRequest() {
            var request = (HttpWebRequest)WebRequest.Create(EoriUri);
            request.Timeout = 15000;
            request.Headers.Add("SOAP:Action");
            request.ContentType = "text/xml;charset=\"utf-8\"";
            request.Accept = "text/xml";
            request.Method = "POST";
            return request;
        }

        private static EoriValidationResponse ParseResponse(string responseString) {
            var xml = new XmlDocument();
            xml.LoadXml(responseString);
            var manager = new XmlNamespaceManager(xml.NameTable);
            manager.AddNamespace("S", "http://schemas.xmlsoap.org/soap/envelope/");
            manager.AddNamespace("r", "http://eori.ws.eos.dds.s/");
            var vatResultNode = xml.SelectSingleNode("//S:Envelope/S:Body/r:validateEORIResponse", manager);
            if (vatResultNode == null) {
                var faultCode = xml.SelectSingleNode("//S:Envelope/S:Body/S:Fault/faultstring", manager)?.InnerText;
                var error = faultCode ?? "";
                var errorMessage = ViesErrorResponse.GetErrorMessage(error);
                return new EoriValidationResponse(error, errorMessage);
            }
            var eoriNumber = vatResultNode.SelectSingleNode("//eori", manager)?.InnerText ?? "";
            var status = (vatResultNode.SelectSingleNode("//status")?.InnerText ?? "").To<int>();
            var statusDescr = vatResultNode.SelectSingleNode("//statusDescr")?.InnerText ?? "";
            var name = vatResultNode.SelectSingleNode("//name", manager)?.InnerText ?? "";
            var street = vatResultNode.SelectSingleNode("//street", manager)?.InnerText ?? "";
            var postalCode = vatResultNode.SelectSingleNode("//postalCode", manager)?.InnerText ?? "";
            var city = vatResultNode.SelectSingleNode("//city", manager)?.InnerText ?? "";
            var country = vatResultNode.SelectSingleNode("//country", manager)?.InnerText ?? "";
            var requestDate = DateTimeHelpers.Parse(vatResultNode.SelectSingleNode("//requestDate", manager)?.InnerText ?? "");
            return new EoriValidationResponse(eoriNumber, status, statusDescr, name, street, postalCode, city, country, requestDate);
        }

        private static byte[] GetRequestData(EoriNumber eoriNumber) {
            var sb = new StringBuilder();
            sb.AppendLine(" <soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">");
            sb.AppendLine("     <soap:Body>");
            sb.AppendLine("         <validateEORI xmlns=\"http://eori.ws.eos.dds.s/\">");
            sb.AppendLine("             <eori>{0}</eori>", eoriNumber.Eori);
            sb.AppendLine("         </validateEORI>");
            sb.AppendLine("     </soap:Body>");
            sb.AppendLine(" </soap:Envelope>");
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}