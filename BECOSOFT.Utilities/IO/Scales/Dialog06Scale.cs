using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BECOSOFT.Utilities.IO.Scales {
    /// <summary>
    /// <see cref="Scale"/> implementation for the Dialog06 scale protocol.
    /// This scale works on a baud rate of 9600, odd parity, 7 databits and one stop bit.
    /// <para>
    /// The implementation for this scale is based on Dialog06 protocol"
    /// </para>
    /// </summary>
    public class Dialog06Scale : Scale {
        private const int MaxKiloGramPrice = 999999;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly byte[] FirstMessage = { SerialConstants.EOT, SerialConstants.STX, 0x32, 0x30, SerialConstants.ESC, 0x31, SerialConstants.ETX };  // logic version number on
        private static readonly byte[] SecondMessage = { SerialConstants.EOT, SerialConstants.STX, 0x32, 0x30, SerialConstants.ESC, 0x30, SerialConstants.ETX }; // logic version number off
        private static readonly byte[] StatusRequestMessageAfterNAK = { SerialConstants.EOT, SerialConstants.STX, 0x30, 0x38, SerialConstants.ETX };
        private static readonly byte[] WeightRequestMessage = { SerialConstants.EOT, SerialConstants.ENQ };
        private static readonly byte[] ChecksumRequestMessage = { SerialConstants.EOT, SerialConstants.STX, 0x31, 0x30, SerialConstants.ESC };
        private static readonly byte[] UnitPriceAndTareRequestMessage = { SerialConstants.EOT, SerialConstants.STX, 0x30, 0x33, SerialConstants.ESC };

        private readonly SerialPort _serialPort;
        private ScaleWeightPolling _polling;
        private ConcurrentQueue<byte> _queue;

        private List<byte> _currentMessage;
        private bool _isWriting;

        private readonly int _kgPrice;
        private readonly int _tare;

        public Dialog06Scale(SerialPortSettings serialPortSettings) {
            _kgPrice = 1; // not implemented currently
            _tare = 0;    // not implemented currently
            _serialPort = serialPortSettings.GetSerialPort(serialPortSettings);
            _serialPort.DataReceived += ReceiveData;
            _serialPort.ErrorReceived += (sender, args) => {
                Debug.WriteLine(args.EventType);
            };
            _queue = new ConcurrentQueue<byte>();
        }

        public override ScaleType Type => ScaleType.Dialog06;
        public override bool IsPolling { get; protected set; }

        public override void StartPolling(ScaleWeightPolling polling = ScaleWeightPolling.Continuous) {
            if (_serialPort.IsOpen) {
                _serialPort.Close();
            }
            _polling = polling;
            _serialPort.Open();
            RequestWeight();
        }

        private void RequestWeight(int? msDelay = null) {
            IsPolling = true;
            if (msDelay > 0) {
                Task.Run(async () => {
                    await Task.Delay(msDelay.Value);
                    StartWeighing();
                });
                return;
            }
            StartWeighing();
        }

        private void SignalWeight(decimal? weight) {
            OnWeightReceived(new ScaleEventArgs(weight, MetricWeightUnit.Gram));
            if (_polling == ScaleWeightPolling.Once) {
                _queue = new ConcurrentQueue<byte>();
                _serialPort.Close();
                IsPolling = false;
            } else {
                RequestWeight(150);
            }
        }

        private void ReceiveData(object sender, SerialDataReceivedEventArgs e) {
            var sp = sender as SerialPort;
            if (sp == null || !sp.IsOpen) { return; }
            var buffer = ReadBuffer();
            foreach (var b in buffer) {
                _queue.Enqueue(b);
            }
            if (!_queue.IsEmpty) {
                ReadQueue();
            }
        }

        private async void ReadQueue() {
            while (_queue.TryDequeue(out var b)) {
                switch (b) {
                    case SerialConstants.STX:
                        // Start of message encountered, starting new message
                        _currentMessage = new List<byte> { b };
                        break;
                    case SerialConstants.ETX:
                        // End of message encountered
                        if (_currentMessage == null) { continue; }
                        _currentMessage.Add(b);
                        var currentMessage = _currentMessage.ToArray();
                        _currentMessage = null;
                        ProcessMessage(currentMessage);
                        break;
                    case SerialConstants.ACK:
                        Write(WeightRequestMessage);
                        await Task.Delay(500);
                        break;
                    case SerialConstants.NAK:
                        Write(StatusRequestMessageAfterNAK);
                        break;
                    default:
                        // byte of transmission
                        if (_currentMessage == null) { continue; }
                        _currentMessage.Add(b);
                        break;
                }
            }
        }

        private void ProcessMessage(byte[] bytes) {
            var messageString = string.Join("", bytes.Select(b => b.ToString("X2")));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: M: {1}", DateTime.Now, messageString);
            Logger.Trace("Scale message: {0}", messageString);
            if (bytes[1] == 0x30 && bytes[2] == 0x32) {
                var weightBytes = bytes.Skip(6).Take(5).Select(b => (char)b).ToArray();
                var weightString = string.Join("", weightBytes).RemoveControlCharacters();
                var weight = weightString.To<decimal?>();
                SignalWeight(weight);
            } else if (bytes[1] == 0x30 && bytes[2] == 0x39) {
                var errorBytes = bytes.Skip(bytes.Length - 3).Take(2).Select(b => (char)b).ToArray();
                var errorCode = new string(errorBytes);
                var errorMessage = ParseErrorMessage(errorCode);
                Debug.WriteLine("{0:HH:mm:ss.ffffff}: E: {1} - {2}", DateTime.Now, errorCode, errorMessage);
                Logger.Error("Error from scale: {0} - {1}", errorCode, errorMessage);
                RequestWeight(150);
            } else if (bytes[1] == 0x31 && bytes[2] == 0x31) {
                var subCommandCode = ((char)bytes[4]).ToString();
                switch (subCommandCode) {
                    case "0": // fail
                        break;
                    case "1": // success
                        StartWeighing();
                        break;
                    case "2": // authenticate
                        Authenticate(bytes.Skip(bytes.Length - 3).Take(2).ToArray());
                        break;
                }
            }
        }

        private void StartWeighing() {
            if (_kgPrice > MaxKiloGramPrice) {
                Logger.Error("Invalid kg price: {0} (above > {1})", _kgPrice, MaxKiloGramPrice);
                return;
            }
            var bytes = new List<byte>(UnitPriceAndTareRequestMessage);
            bytes.AddRange(Encoding.ASCII.GetBytes(_kgPrice.ToString().PadLeft(6, '0')));
            bytes.Add(SerialConstants.ESC);
            bytes.AddRange(Encoding.ASCII.GetBytes(_tare.ToString().PadLeft(4, '0')));
            bytes.Add(SerialConstants.ETX);
            Write(bytes.ToArray());
        }

        private void Authenticate(byte[] bytes) {
            var left = bytes[0];
            var right = bytes[1];
            uint cs = 0x4711;
            uint kw = CheckCSKW(cs << 16);

            var csTimes = int.Parse(((char)left).ToString(), System.Globalization.NumberStyles.HexNumber);
            var kwTimes = int.Parse(((char)right).ToString(), System.Globalization.NumberStyles.HexNumber);

            cs = RotateLeft(cs, csTimes);
            kw = RotateRight(kw, kwTimes);

            var csHex = cs.ToString("X");
            var kwHex = kw.ToString("X");

            if (csHex.Length > kwHex.Length)
                kwHex = kwHex.PadLeft(csHex.Length, '0');

            if (kwHex.Length > csHex.Length)
                csHex = csHex.PadLeft(kwHex.Length, '0');

            var csBytes = StringToByteArray(csHex);
            var kwBytes = StringToByteArray(kwHex);

            var listMsg = new List<byte>(ChecksumRequestMessage);

            listMsg.AddRange(csBytes);
            listMsg.AddRange(kwBytes);

            listMsg.Add(SerialConstants.ETX);

            Write(listMsg.ToArray());
        }

        private async Task WriteWithDelay(byte[] bytes, int msDelay) {
            await Task.Delay(msDelay);
            Write(bytes);
        }

        private void Write(byte b) {
            Write(new[] { b });
        }

        private void Write(byte[] bytesToWrite) {
            if (_isWriting || !IsPolling || !_serialPort.IsOpen) { return; }
            _isWriting = true;
            _serialPort.Write(bytesToWrite, 0, bytesToWrite.Length);
            var formattedBytes = string.Join("", bytesToWrite.Select(b => $"{b:X2}"));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: W: {1}", DateTime.Now, formattedBytes);
            Logger.Trace("Written: {0}", formattedBytes);
            _isWriting = false;
        }

        private byte[] ReadBuffer() {
            if (!_serialPort.IsOpen || _serialPort.BytesToRead == 0) { return Array.Empty<byte>(); }
            var buffer = new byte[_serialPort.BytesToRead];
            _serialPort.Read(buffer, 0, buffer.Length);
            var logString = string.Join("", buffer.Select(b => b.ToString("X2")));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: R: {1}", DateTime.Now, logString);
            Logger.Trace("Read: {0}", logString);
            return buffer;
        }

        public override void Dispose() {
            IsPolling = false;
            Write(SerialConstants.EOT);
            _serialPort?.Close();
        }

        public static SerialPortSettings GetDefaultSerialPortSettings(string port) {
            return new SerialPortSettings {
                ComPort = port,
                BaudRate = 9600,
                Parity = Parity.Odd,
                DataBits = 7,
                StopBits = StopBits.One
            };
        }
        
        private static string ParseErrorMessage(string errorCode) {
            var errorMessage = "no error";
            switch (errorCode) {
                case "01":
                    errorMessage = "general error";
                    break;
                case "02":
                    errorMessage = "parity error or buffer overflow";
                    break;
                case "10":
                    errorMessage = "invalid record no.";
                    break;
                case "11":
                    errorMessage = "invalid unit price";
                    break;
                case "12":
                    errorMessage = "invalid tare value";
                    break;
                case "13":
                    errorMessage = "invalid text";
                    break;
                case "20":
                    errorMessage = "scale is still in motion(no standstill)";
                    break;
                case "21":
                    errorMessage = "scale wasn’t in motion since last operation";
                    break;
                case "22":
                    errorMessage = "measurement is not yet finished";
                    break;
                case "30":
                    errorMessage = "weight is less than minimum weight";
                    break;
                case "31":
                    errorMessage = "scale is less than 0";
                    break;
                case "32":
                    errorMessage = "scale is overloaded";
                    break;
            }
            return errorMessage;
        }

        #region Autenticate

        private static uint CheckCSKW(ulong cskw) {
            byte bIndex;
            ulong ulPolynome = 0xC3518000L; // 0x186A3000L normalized;
            ulong ulMask = 0x80000000L;
            for (bIndex = 0; bIndex <= 15; bIndex++) {
                if ((cskw & ulMask) != 0) {
                    cskw ^= ulPolynome;
                }
                ulPolynome >>= 1;
                ulMask >>= 1;
            }
            return (uint)cskw;
        }

        private static uint RotateLeft(uint value, int z1) {
            return (((value << z1) & 0xffff0000) >> 16) + ((value << z1) & 0x0000ffff);
        }

        private static uint RotateRight(uint value, int count) {
            return (((value << 16) >> count) & 0x0000ffff) + ((value >> count) & 0x0000ffff);
        }

        private static byte[] StringToByteArray(string hex) {
            return hex.Select(c => (byte)c).ToArray();
        }

        #endregion
    }
}