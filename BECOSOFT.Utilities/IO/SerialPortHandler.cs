using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace BECOSOFT.Utilities.IO {
    public class SerialPortHandler {
        /// <summary>
        /// Your serial port
        /// </summary>
        private SerialPort _serialPort;
        private int _timeOut;
        private int _timeOutDefault;
        private AutoResetEvent _receiveNow;
        /// <summary>
        /// Possible device end responses such as \r\nOK\r\n, \r\nERROR\r\n, etc.
        /// </summary>
        private string[] _endResponses;
        /// <summary>
        /// Name of the serial port
        /// </summary>
        public string PortName => _serialPort.PortName;

        public void SetPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, int timeOut, string[] endResponses = null) {
            _timeOut = timeOut;
            _timeOutDefault = timeOut;
            _serialPort = new SerialPort(portName, baudRate) {
                Parity = parity,
                Handshake = Handshake.None,
                DataBits = dataBits,
                StopBits = stopBits,
                RtsEnable = true,
                DtrEnable = true,
                WriteTimeout = _timeOut,
                ReadTimeout = _timeOut
            };

            _endResponses = endResponses ?? new string[0];
        }

        public bool Open() {
            try {
                if (_serialPort == null) { return false; }

                if (!_serialPort.IsOpen) {
                    _receiveNow = new AutoResetEvent(false);
                    _serialPort.Open();
                    _serialPort.DataReceived += SerialPort_DataReceived;
                }

                return true;
            } catch {
                return false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            if (e.EventType == SerialData.Chars) {
                _receiveNow.Set();
            }
        }

        public bool Close() {
            try {
                if (_serialPort == null || !_serialPort.IsOpen) { return false; }

                _serialPort.Close();
                return true;
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Executes a command and waits for a response.
        /// </summary>
        /// <param name="cmd">The command to execute</param>
        /// <param name="reponseTimeOut">(Optional) Time-out value in ms. An empty response will be returned after this time-out. Default is 3000ms.</param>
        /// <returns>The response.</returns>
        public char[] ExecuteCommand(char[] cmd, int reponseTimeOut = 3000) {
            _serialPort.DiscardOutBuffer();
            _serialPort.DiscardInBuffer();
            _receiveNow.Reset();
            _serialPort.Write(cmd, 0, cmd.Length); // Sometimes  + "\r" is needed. Depends on the device

            var input = ReadResponse(reponseTimeOut); // Returns device response whenever you execute a command

            _timeOut = _timeOutDefault;

            return input;
        }

        private char[] ReadResponse(int responseTimeOut) {
            var buffer = string.Empty;
            var stopWatch = new Stopwatch();
            var reading = false;

            stopWatch.Start();
            try {
                do {
                    if (!_receiveNow.WaitOne(_timeOut, false)) { continue; }
                    reading = true;
                    var t = _serialPort.ReadExisting();
                    buffer += t;

                } while ((stopWatch.Elapsed.TotalMilliseconds < responseTimeOut || reading) &&
                         !_endResponses.Any(r => buffer.EndsWith(r, StringComparison.OrdinalIgnoreCase))); // Read while end responses are not yet received and time-out hasn't passed
                reading = false;
            } catch {
                buffer = string.Empty;
            }
            stopWatch.Stop();

            return buffer.ToCharArray();
        }
    }
}