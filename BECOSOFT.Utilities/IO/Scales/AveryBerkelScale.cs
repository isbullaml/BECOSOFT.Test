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
    /// <see cref="Scale"/> implementation for the Avery Berkel scale.
    /// This scale works on a baud rate of 2400, even parity, 7 databits and one stop bit.
    /// <para>
    /// The implementation for this scale is based on ECR Type 0/1, taken from document "OPOS scale Protocol sheet_v14 - 20160726-1.pdf"
    /// </para>
    /// </summary>
    public class AveryBerkelScale : Scale {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly SerialPort _serialPort;
        private ScaleWeightPolling _polling;
        private ConcurrentQueue<byte> _queue;

        private List<byte> _currentMessage;
        private bool _isWriting;

        public override ScaleType Type => ScaleType.AveryBerkel;

        public AveryBerkelScale(SerialPortSettings serialPortSettings) {
            _serialPort = serialPortSettings.GetSerialPort(serialPortSettings);
            _serialPort.DataReceived += ReceiveData;
            _queue = new ConcurrentQueue<byte>();
        }

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
                Task.Run(async () => await WriteWithDelay(SerialConstants.ENQ, msDelay.Value));
                return;
            }
            Write(SerialConstants.ENQ);
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

        private void ReceiveData(object sender, EventArgs eventArgs) {
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
                    case SerialConstants.ACK:
                        // ENQ acknowledged, sending DC2 (request for weight)
                        Write(SerialConstants.DC2);
                        break;
                    case SerialConstants.NAK:
                        // ENQ or DC2 not acknowledged, sending ENQ with delay
                        await WriteWithDelay(SerialConstants.ENQ, 150);
                        break;
                    case SerialConstants.STX:
                        // Start of message encountered, starting new message
                        _currentMessage = new List<byte> { b };
                        break;
                    case SerialConstants.ETX:
                        // End of message encountered
                        if (_currentMessage == null) { continue; }
                        _currentMessage.Add(b);
                        var result = ProcessMessage(_currentMessage.ToArray());
                        _currentMessage = null;
                        if (!result) {
                            await WriteWithDelay(SerialConstants.ENQ, 150);
                        }
                        break;
                    case SerialConstants.BEL:
                        SignalWeight(null);
                        break;
                    default:
                        // byte of transmission
                        if (_currentMessage == null) { continue; }
                        _currentMessage.Add(b);
                        break;
                }
            }
        }

        private bool ProcessMessage(byte[] bytes) {
            var messageString = string.Join("", bytes.Select(b => b.ToString("X2")));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: M: {1}", DateTime.Now, messageString);
            Logger.Trace("Scale message: {0}", messageString);
            if (!IsValidMessage(bytes)) {
                return false;
            }
            var scaleTypeIdentifier = bytes[1];
            var scaleType = GetScaleType(scaleTypeIdentifier);
            var weightBytes = bytes.Skip(2).Take(5).ToArray();
            var weightText = Encoding.ASCII.GetString(weightBytes);
            decimal parsedWeight;
            if (decimal.TryParse(weightText, out parsedWeight)) {
                Debug.WriteLine("{0:HH:mm:ss.ffffff}: S: {1} (type: {2})", DateTime.Now, parsedWeight, scaleType);
                Logger.Trace("Parsed weight: {0} (type: {1})", parsedWeight, scaleType);
                SignalWeight(parsedWeight);
                return true;
            }
            return false;
        }

        private static bool IsValidMessage(byte[] bytes) {
            var blockCheckCharackter = bytes[bytes.Length - 2];
            var cc = 0;
            for (var i = 1; i < bytes.Length - 2; i++) {
                cc ^= bytes[i];
            }
            return cc == blockCheckCharackter;
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

        private async Task WriteWithDelay(byte b, int msDelay) {
            await Task.Delay(msDelay);
            Write(b);
        }

        private void Write(byte byteToWrite) {
            if (_isWriting || !IsPolling || !_serialPort.IsOpen) { return; }
            _isWriting = true;
            var bytes = new[] { byteToWrite };
            _serialPort.Write(bytes, 0, bytes.Length);
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: W: {1:X2}", DateTime.Now, byteToWrite);
            Logger.Trace("Written: {0:X2}", byteToWrite);
            _isWriting = false;
        }

        private static string GetScaleType(byte b) {
            string type;
            switch (b) {
                case 0x47:
                    type = "2kg";
                    break;
                case 0x48:
                    type = "5kg";
                    break;
                case 0x43:
                    type = "6kg";
                    break;
                case 0x49:
                    type = "10kg";
                    break;
                case 0x41:
                    type = "15kg";
                    break;
                case 0x4A:
                    type = "20kg";
                    break;
                case 0x50:
                    type = "25kg";
                    break;
                case 0x42:
                    type = "30kg";
                    break;
                case 0x4F:
                    type = "60kg";
                    break;
                default:
                    return string.Empty;
            }
            return type;
        }

        public override void Dispose() {
            IsPolling = false;
            _serialPort?.Close();
        }

        public static SerialPortSettings GetDefaultSerialPortSettings(string port) {
            return new SerialPortSettings {
                ComPort = port,
                BaudRate = 2400,
                Parity = Parity.Even,
                DataBits = 7,
                StopBits = StopBits.One
            };
        }
    }
}