using System.IO.Ports;

namespace BECOSOFT.Utilities.IO.Scales {
    public class SerialPortSettings {
        public string ComPort { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }

        public SerialPort GetSerialPort(SerialPortSettings serialPortSettings) {
            return new SerialPort(serialPortSettings.ComPort, serialPortSettings.BaudRate) {
                Parity = serialPortSettings.Parity,
                DataBits = serialPortSettings.DataBits,
                StopBits = serialPortSettings.StopBits,
            };
        }
    }
}