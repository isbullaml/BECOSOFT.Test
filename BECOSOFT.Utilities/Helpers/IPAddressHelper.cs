using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace BECOSOFT.Utilities.Helpers {

    /// <summary>
    /// Helper class to retrieve IP address information
    /// <para>Source: https://stackoverflow.com/questions/6803073/get-local-ip-address </para>
    /// </summary>
    public static class IPAddressHelper {
        /// <summary>
        /// Retrieves all local IPv4 addresses for the given <see cref="interfaceType"/>.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static IReadOnlyList<string> GetLocalIPv4Addresses(NetworkInterfaceType interfaceType = NetworkInterfaceType.Ethernet) {
            try {
                var ipAddressList = GetAllLocalIPAddresses(interfaceType, AddressFamily.InterNetwork);
                var result = new List<string>(ipAddressList);
                return result.AsReadOnly();
            } catch (NetworkInformationException) {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the first local IPv4 addresses for the given <see cref="NetworkInterface"/>
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static string GetLocalIPv4Address(NetworkInterfaceType interfaceType = NetworkInterfaceType.Ethernet) {
            try {
                var ipAddressList = GetAllLocalIPAddresses(interfaceType, AddressFamily.InterNetwork);
                return ipAddressList.FirstOrDefault();
            } catch (NetworkInformationException) {
                return null;
            }
        }

        private static IEnumerable<string> GetAllLocalIPAddresses(NetworkInterfaceType interfaceType, AddressFamily addressFamily) {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces) {
                if (networkInterface.NetworkInterfaceType != interfaceType || networkInterface.OperationalStatus != OperationalStatus.Up) {
                    continue;
                }
                foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses) {
                    if (ip.Address.AddressFamily == addressFamily) {
                        yield return ip.Address.ToString();
                    }
                }
            }
        }
    }
}
