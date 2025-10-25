using System.Diagnostics.CodeAnalysis ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak ;

[ ExcludeFromCodeCoverage ]
public class DeskConnectorSubjects ( Func < ISubject < IEnumerable < byte > > > subjectFactory ,
                                     ISubject < uint > heightChanged ,
                                     ISubject < int > speedChanged ,
                                     ISubject < bool > refreshedChanged ,
                                     ISubject < HeightSpeedDetails > heightAndSpeedChanged ) : IDeskConnectorSubjects
{
    public Func < ISubject < IEnumerable < byte > > > SubjectFactory        { get ; } = subjectFactory ;
    public ISubject < uint >                          HeightChanged         { get ; } = heightChanged ;
    public ISubject < int >                           SpeedChanged          { get ; } = speedChanged ;
    public ISubject < bool >                          RefreshedChanged      { get ; } = refreshedChanged ;
    public ISubject < HeightSpeedDetails >            HeightAndSpeedChanged { get ; } = heightAndSpeedChanged ;
}