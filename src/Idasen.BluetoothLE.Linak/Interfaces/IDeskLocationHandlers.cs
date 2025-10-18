namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskLocationHandlers
{
    public IDeskHeightAndSpeed                   HeightAndSpeed  { get ; }
    public IInitialHeightAndSpeedProviderFactory ProviderFactory { get ; }
}
