using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Characteristics.Common ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

public class GenericAccess
    : UnknownBase ,
      IGenericAccess
{
    internal const string               Message = "Can't use an unknown instance" ;
    public         IEnumerable < byte > RawResolution { get ; } = RawArrayEmpty ;
    public         IEnumerable < byte > RawParameters { get ; } = RawArrayEmpty ;
    public         IEnumerable < byte > RawAppearance { get ; } = RawArrayEmpty ;
    public         IEnumerable < byte > RawDeviceName { get ; } = RawArrayEmpty ;

    public ISubject < IEnumerable < byte > > AppearanceChanged => throw new NotInitializeException ( Message ) ;

    public ISubject < IEnumerable < byte > > ParametersChanged => throw new NotInitializeException ( Message ) ;

    public ISubject < IEnumerable < byte > > ResolutionChanged => throw new NotInitializeException ( Message ) ;

    public ISubject < IEnumerable < byte > > DeviceNameChanged => throw new NotInitializeException ( Message ) ;

    public Guid GattServiceUuid { get ; } = Guid.Empty ;
}