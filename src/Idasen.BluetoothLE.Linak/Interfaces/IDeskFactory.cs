namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskFactory
{
    Task < IDesk > CreateAsync ( ulong address ) ;
}