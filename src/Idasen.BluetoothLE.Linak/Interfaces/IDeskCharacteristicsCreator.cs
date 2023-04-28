using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    /// <summary>
    ///     Creates and adds desk characteristics to a given characteristics.
    /// </summary>
    public interface IDeskCharacteristicsCreator
    {
        /// <summary>
        ///     Create all the desk characteristics found on the given device
        ///     and add them to the given characteristics.
        /// </summary>
        /// <param name="characteristics">
        ///     The characteristics to add the create desk characteristics to.
        /// </param>
        /// <param name="device">
        ///     The device providing the characteristics.
        /// </param>
        void Create ( IDeskCharacteristics characteristics ,
                      IDevice           device ) ;
    }
}