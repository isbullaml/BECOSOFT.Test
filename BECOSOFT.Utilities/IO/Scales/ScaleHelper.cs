using System;

namespace BECOSOFT.Utilities.IO.Scales {
    public static class ScaleHelper {
        public static Scale GetScale(ScaleType type, SerialPortSettings settings) {
            switch (type) {
                case ScaleType.AveryBerkel:
                    return new AveryBerkelScale(settings);
                case ScaleType.Dialog06:
                    return new Dialog06Scale(settings);
                case ScaleType.L2MettlerToledo:
                    return new L2MettlerToledoScale(settings);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static Scale GetScaleWithDefaultSettings(ScaleType type, string port) {
            SerialPortSettings settings;
            switch (type) {
                case ScaleType.AveryBerkel:
                    settings = AveryBerkelScale.GetDefaultSerialPortSettings(port);
                    break;
                case ScaleType.Dialog06:
                    settings = Dialog06Scale.GetDefaultSerialPortSettings(port);
                    break;
                case ScaleType.L2MettlerToledo:
                    settings = L2MettlerToledoScale.GetDefaultSerialPortSettings(port);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return GetScale(type, settings);
        }
    }
}