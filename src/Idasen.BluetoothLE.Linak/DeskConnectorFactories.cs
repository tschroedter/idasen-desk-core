using Idasen.BluetoothLE.Linak.Interfaces;

namespace Idasen.BluetoothLE.Linak
{
    public class DeskConnectorFactories : IDeskConnectorFactories
    {
        public DeskConnectorFactories(
            IDeskHeightAndSpeedFactory heightAndSpeedFactory,
            IDeskCommandExecutorFactory commandExecutorFactory,
            IDeskMoverFactory moverFactory,
            IDeskLockerFactory lockerFactory)
        {
            HeightAndSpeedFactory = heightAndSpeedFactory;
            CommandExecutorFactory = commandExecutorFactory;
            MoverFactory = moverFactory;
            LockerFactory = lockerFactory;
        }

        public IDeskHeightAndSpeedFactory HeightAndSpeedFactory { get; }
        public IDeskCommandExecutorFactory CommandExecutorFactory { get; }
        public IDeskMoverFactory MoverFactory { get; }
        public IDeskLockerFactory LockerFactory { get; }
    }
}
