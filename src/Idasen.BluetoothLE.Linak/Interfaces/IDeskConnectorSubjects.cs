using System.Reactive.Subjects ;

namespace Idasen.BluetoothLE.Linak.Interfaces ;

public interface IDeskConnectorSubjects
{
    Func < ISubject < IEnumerable < byte > > > SubjectFactory        { get ; }
    ISubject < uint >                          HeightChanged         { get ; }
    ISubject < int >                           SpeedChanged          { get ; }
    ISubject < bool >                          RefreshedChanged      { get ; }
    ISubject < HeightSpeedDetails >            HeightAndSpeedChanged { get ; }
}