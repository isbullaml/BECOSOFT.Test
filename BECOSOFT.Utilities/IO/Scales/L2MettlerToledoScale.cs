using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
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
    /// <see cref="Scale"/> implementation for the L2 Mettler Toledo scale protocol configured on OHAUS scales.
    /// This scale works on a baud rate of 9600, none parity, 8 databits and one stop bit.
    /// </summary>
    public class L2MettlerToledoScale : Scale {
        private const int MaxKiloGramPrice = 99999;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly byte[] AckMessage = { SerialConstants.ACK, SerialConstants.CR, SerialConstants.LF };
        private static readonly byte[] NakMessage = { SerialConstants.NAK, SerialConstants.CR, SerialConstants.LF };
        private static readonly byte[] UnitPriceAndTareRequestMessage = { 0x47 };

        private readonly SerialPort _serialPort;
        private ScaleWeightPolling _polling;
        private ConcurrentQueue<byte> _queue;

        private List<byte> _currentMessage;
        private bool _isWriting;

        private readonly int _kgPrice;

        public L2MettlerToledoScale(SerialPortSettings serialPortSettings) {
            _kgPrice = 1; // not implemented currently
            _serialPort = serialPortSettings.GetSerialPort(serialPortSettings);
            _serialPort.DataReceived += ReceiveData;
            _serialPort.ErrorReceived += (sender, args) => {
                Debug.WriteLine(args.EventType);
            };
            _queue = new ConcurrentQueue<byte>();
        }

        public override ScaleType Type => ScaleType.L2MettlerToledo;
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

        private void SignalWeight(decimal? weight, decimal? price) {
            OnWeightReceived(new ScaleEventArgs(weight, MetricWeightUnit.Gram, price));
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

        private void ReadQueue() {
            while (_queue.TryDequeue(out var b)) {
                switch (b) {
                    case SerialConstants.LF: {
                            if (_currentMessage == null) {
                                continue;
                            }
                            // End of message encountered
                            _currentMessage.Add(b);
                            HandleMessageBytes();
                            break;
                        }
                    default:
                        // byte of transmission
                        if (_currentMessage == null) {
                            // Start of message encountered, starting new message
                            _currentMessage = new List<byte>();
                        }
                        _currentMessage.Add(b);
                        break;
                }
            }
            if (_currentMessage.HasAny()) {
                HandleMessageBytes();
            }
        }

        private void HandleMessageBytes() {
            if (_currentMessage.IsEmpty()) { return; }
            var currentMessage = _currentMessage.ToArray();
            var finishedMessage = GetFinishedMessage(currentMessage);
            if (finishedMessage.Length == 0) { return; }
            _currentMessage.RemoveRange(0, finishedMessage.Length);

            if (finishedMessage.Length == 3) {
                if (finishedMessage.First() == 0x44) {
                    Write(AckMessage);
                    return;
                }
                if (finishedMessage.SequenceEqual(NakMessage)) {
                    Write(AckMessage);
                    RequestWeight();
                    return;
                }
                if (finishedMessage.SequenceEqual(AckMessage)) { return; }
            }
            var isSuccess = ProcessMessage(finishedMessage);
            if (isSuccess) {
                Write(AckMessage);
            } else {
                Write(NakMessage);
            }
        }

        private byte[] GetFinishedMessage(byte[] bytes) {
            if (bytes.Length < 2) {
                //     Logger.Trace("Invalid message length");
                return Array.Empty<byte>();
            }
            var message = new List<byte>();
            for (var i = 0; i < bytes.Length - 1; i++) {
                message.Add(bytes[i]);
                if (bytes[i] == SerialConstants.CR && bytes[i + 1] == SerialConstants.LF) {
                    message.Add(bytes[i + 1]);
                    return message.ToArray();
                }
            }
            return Array.Empty<byte>();
        }

        private bool ProcessMessage(byte[] bytes) {
            var messageString = string.Join("", bytes.Select(b => b.ToString("X2")));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: M: {1}", DateTime.Now, messageString);
            Logger.Trace("Scale message: {0}", messageString);
            if (bytes.Length < 2) {
                Logger.Trace("Invalid message length");
                return false;
            }
            var secondToLast = bytes.Skip(bytes.Length - 2).First();
            var last = bytes.Last();
            var isFinished = secondToLast == SerialConstants.CR && last == SerialConstants.LF;
            if (!isFinished) {
                Logger.Trace("Invalid terminator bytes, expected: {0:X2} and {1:X2}, received: {2:X2} and {3:X2}", SerialConstants.CR, SerialConstants.LF, secondToLast, last);
                return false;
            }
            var dataBytes = bytes.Take(bytes.Length - 2).ToList();
            if (dataBytes.IsEmpty()) {
                Logger.Trace("Invalid message length");
                return false;
            }
            var indexOfSpace = dataBytes.IndexOf((byte)0x20);
            if (indexOfSpace < 0) {
                Logger.Trace("Invalid message received, no space present");
                return false;
            }
            var weightBytes = dataBytes.Take(indexOfSpace).ToArray();
            var weightString = string.Join("", weightBytes.Select(Convert.ToChar)).RemoveControlCharacters();
            var weight = weightString.To<decimal?>();
            var priceBytes = dataBytes.Skip(indexOfSpace).ToArray();
            var priceString = string.Join("", priceBytes.Select(Convert.ToChar)).RemoveControlCharacters();
            var price = priceString.To<decimal?>();
            if (price.HasValue) {
                price = price.Value / 100m;
            }
            Logger.Trace("Read {0:F0}g and price: {1:F2}", weight.GetValueOrDefault(), price.GetValueOrDefault());
            SignalWeight(weight, price);
            return true;
        }

        private void StartWeighing() {
            if (_kgPrice > MaxKiloGramPrice) {
                Logger.Error("Invalid kg price: {0} (above > {1})", _kgPrice, MaxKiloGramPrice);
                return;
            }
            var bytes = new List<byte>(UnitPriceAndTareRequestMessage);
            bytes.AddRange(Encoding.ASCII.GetBytes(_kgPrice.ToString().PadLeft(5, '0')));
            bytes.Add(SerialConstants.CR);
            bytes.Add(SerialConstants.LF);
            Write(bytes.ToArray());
        }

        private void Write(byte b) {
            Write(new[] { b });
        }

        private void Write(byte[] bytesToWrite) {
            if (_isWriting || !IsPolling || !_serialPort.IsOpen) { return; }
            _isWriting = true;
            var formattedBytes = string.Join("", bytesToWrite.Select(b => $"{b:X2}"));
            Debug.WriteLine("{0:HH:mm:ss.ffffff}: W: {1}", DateTime.Now, formattedBytes);
            Logger.Trace("Written: {0}", formattedBytes);
            _serialPort.Write(bytesToWrite, 0, bytesToWrite.Length);
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
            _serialPort?.Close();
        }

        public static SerialPortSettings GetDefaultSerialPortSettings(string port) {
            return new SerialPortSettings {
                ComPort = port,
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
            };
        }
    }
}