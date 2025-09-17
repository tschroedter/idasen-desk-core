using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

[ Intercept ( typeof ( LogAspect ) ) ]
/// <summary>
///     Adapts the LINAK Reference Output characteristic into strongly-typed height and speed observables and values.
/// </summary>
public class DeskHeightAndSpeed
    : IDeskHeightAndSpeed
{
    public delegate IDeskHeightAndSpeed Factory ( IReferenceOutput referenceOutput ) ;

    private readonly IRawValueToHeightAndSpeedConverter _converter ;

    private readonly ILogger _logger ;
    private readonly IReferenceOutput _referenceOutput ;
    private readonly IScheduler _scheduler ;
    private readonly ISubject < uint > _subjectHeight ;
    private readonly ISubject < HeightSpeedDetails > _subjectHeightAndSpeed ;
    private readonly ISubject < int > _subjectSpeed ;

    private IDisposable? _subscriber ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskHeightAndSpeed"/> class.
    /// </summary>
    public DeskHeightAndSpeed ( ILogger logger ,
                                IScheduler scheduler ,
                                IReferenceOutput referenceOutput ,
                                IRawValueToHeightAndSpeedConverter converter ,
                                ISubject < uint > subjectHeight ,
                                ISubject < int > subjectSpeed ,
                                ISubject < HeightSpeedDetails > subjectHeightAndSpeed )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( referenceOutput ,
                                nameof ( referenceOutput ) ) ;
        Guard.ArgumentNotNull ( converter ,
                                nameof ( converter ) ) ;
        Guard.ArgumentNotNull ( subjectHeight ,
                                nameof ( subjectHeight ) ) ;
        Guard.ArgumentNotNull ( subjectSpeed ,
                                nameof ( subjectSpeed ) ) ;
        Guard.ArgumentNotNull ( subjectHeightAndSpeed ,
                                nameof ( subjectHeightAndSpeed ) ) ;

        _logger = logger ;
        _scheduler = scheduler ;
        _referenceOutput = referenceOutput ;
        _converter = converter ;
        _subjectHeight = subjectHeight ;
        _subjectSpeed = subjectSpeed ;
        _subjectHeightAndSpeed = subjectHeightAndSpeed ;
    }

    /// <inheritdoc />
    public IObservable < uint > HeightChanged => _subjectHeight ;

    /// <inheritdoc />
    public IObservable < int > SpeedChanged => _subjectSpeed ;

    /// <inheritdoc />
    public IObservable < HeightSpeedDetails > HeightAndSpeedChanged => _subjectHeightAndSpeed ;

    /// <inheritdoc />
    public uint Height { get ; private set ; }

    /// <inheritdoc />
    public int Speed { get ; private set ; }

    /// <inheritdoc />
    public async Task Refresh ( )
    {
        await _referenceOutput.Refresh ( ) ;

        Initialize ( ) ;
    }

    /// <inheritdoc />
    public IDeskHeightAndSpeed Initialize ( )
    {
        _subscriber?.Dispose ( ) ;

        _subscriber = _referenceOutput.HeightSpeedChanged
                                      .ObserveOn ( _scheduler )
                                      .Subscribe ( OnHeightSpeedChanged ) ;

        if ( _subscriber is UnknownBase )
        {
            _logger.Warning ( $"{nameof ( _referenceOutput )} is set to Unknown" ) ;
        }

        if ( ! _converter.TryConvert ( _referenceOutput.RawHeightSpeed ,
                                       out var height ,
                                       out var speed ) )
        {
            return this ;
        }

        Height = height ;
        Speed = speed ;

        return this ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _referenceOutput.Dispose ( ) ;
        _subscriber?.Dispose ( ) ;
    }

    private void OnHeightSpeedChanged ( RawValueChangedDetails details )
    {
        if ( ! _converter.TryConvert ( details.Value ,
                                       out var height ,
                                       out var speed ) )
        {
            return ;
        }

        Height = height ;
        Speed = speed ;

        _subjectHeight.OnNext ( Height ) ;
        _subjectSpeed.OnNext ( Speed ) ;

        var value = new HeightSpeedDetails ( details.Timestamp ,
                                             Height ,
                                             Speed ) ;

        _subjectHeightAndSpeed.OnNext ( value ) ;

        _logger.Debug ( $"Height = {Height} (10ths of a millimeter), Speed = {Speed} (100/RPM)" ) ;
    }
}