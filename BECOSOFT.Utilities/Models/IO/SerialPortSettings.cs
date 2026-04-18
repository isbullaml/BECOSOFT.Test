using System.IO.Ports;

namespace BECOSOFT.Utilities.Models.IO {
    public class SerialPortSettings {
        public string Port { get; set; }
        public int BaudRate { get; set; }
        public Parity Parity { get; set; }
        public int DataBits { get; set; }
        public StopBits StopBits { get; set; }
        public Handshake HandShake { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadTimeout { get; set; }

        public SerialPortSettings(string port) {
            Port = port;
            // Default SerialPort settings:
            BaudRate = 9600;
            DataBits = 8;
            Parity = Parity.None;
            StopBits = StopBits.One;
            HandShake = Handshake.None;
        }

        public SerialPort ToSerialPort() {
            var serialPort = new SerialPort(Port, BaudRate, Parity, DataBits, StopBits);
            if (WriteTimeout != 0) {
                serialPort.WriteTimeout = WriteTimeout;
            }
            if (ReadTimeout != 0) {
                serialPort.ReadTimeout = ReadTimeout;
            }
            return serialPort;
        }
    }
}
