namespace Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery
{
    /// <summary>
    ///     Information about a BLE device.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        ///     The time of the broadcast advertisement message of the device.
        /// </summary>
        public IDateTimeOffset BroadcastTime { get ; }

        /// <summary>
        ///     The address of the device.
        /// </summary>
        public ulong Address { get ; }

        /// <summary>
        ///     The name of the device.
        /// </summary>
        public string ? Name { get ; }

        /// <summary>
        ///     The signal strength in dB.
        /// </summary>
        public short RawSignalStrengthInDBm { get ; }

        /// <summary>
        ///     The Mac Address of the device.
        /// </summary>
        string MacAddress { get ; }

        /// <summary>
        ///     The details of the device.
        ///     (ToString() doesn't work because of the Aspects.)
        /// </summary>
        string Details { get ; }
    }
}