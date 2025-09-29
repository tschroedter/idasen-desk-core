using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskHeightAndSpeed
    : IDeskHeightAndSpeed
{
    public delegate IDeskHeightAndSpeed Factory ( IReferenceOutput referenceOutput ) ;

    private readonly IRawValueToHeightAndSpeedConverter _converter ;

    private readonly ILogger                         _logger ;
    private readonly IReferenceOutput                _referenceOutput ;
    private readonly IScheduler                      _scheduler ;
    private readonly ISubject < uint >               _subjectHeight ;
    private readonly ISubject < HeightSpeedDetails > _subjectHeightAndSpeed ;
    private readonly ISubject < int >                _subjectSpeed ;

    private IDisposable ? _subscriber ;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeskHeightAndSpeed" /> class.
    /// </summary>
    public DeskHeightAndSpeed ( ILogger                            logger ,
                                IScheduler                         scheduler ,
                                IReferenceOutput                   referenceOutput ,
                                IRawValueToHeightAndSpeedConverter converter ,
                                ISubject < uint >                  subjectHeight ,
                                ISubject < int >                   subjectSpeed ,
                                ISubject < HeightSpeedDetails >    subjectHeightAndSpeed )
    {
        ArgumentNullException.ThrowIfNull ( logger ) ;
        ArgumentNullException.ThrowIfNull ( scheduler ) ;
        ArgumentNullException.ThrowIfNull ( referenceOutput ) ;
        ArgumentNullException.ThrowIfNull ( converter ) ;
        ArgumentNullException.ThrowIfNull ( subjectHeight ) ;
        ArgumentNullException.ThrowIfNull ( subjectSpeed ) ;
        ArgumentNullException.ThrowIfNull ( subjectHeightAndSpeed ) ;

        _logger                = logger ;
        _scheduler             = scheduler ;
        _referenceOutput       = referenceOutput ;
        _converter             = converter ;
        _subjectHeight         = subjectHeight ;
        _subjectSpeed          = subjectSpeed ;
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
        await _referenceOutput.Refresh ( ).ConfigureAwait ( false ) ;

        Initialize ( ) ;
    }

    /// <inheritdoc />
    public IDeskHeightAndSpeed Initialize ( )
    {
        _subscriber?.Dispose ( ) ;

        _subscriber = _referenceOutput.HeightSpeedChanged
                                      .ObserveOn ( _scheduler )
                                      .Subscribe ( OnHeightSpeedChanged ,
                                                   ex => _logger.Error ( ex ,
                                                                         "Error while handling HeightSpeedChanged" ) ) ;

        if ( _referenceOutput is UnknownBase )
            _logger.Warning ( "{ReferenceOutput} is set to Unknown" ,
                              nameof ( _referenceOutput ) ) ;

        if ( ! _converter.TryConvert ( _referenceOutput.RawHeightSpeed ,
                                       out var height ,
                                       out var speed ) )
            return this ;

        Height = height ;
        Speed  = speed ;

        return this ;
    }

    /// <inheritdoc />
    public void Dispose ( )
    {
        _referenceOutput.Dispose ( ) ;
        _subscriber?.Dispose ( ) ;
        _subscriber = null ;
    }

    private void OnHeightSpeedChanged ( RawValueChangedDetails details )
    {
        if ( ! _converter.TryConvert ( details.Value ,
                                       out var height ,
                                       out var speed ) )
            return ;

        Height = height ;
        Speed  = speed ;

        _subjectHeight.OnNext ( Height ) ;
        _subjectSpeed.OnNext ( Speed ) ;

        var value = new HeightSpeedDetails ( details.Timestamp ,
                                             Height ,
                                             Speed ) ;

        _subjectHeightAndSpeed.OnNext ( value ) ;

        _logger.Debug ( "Height={Height} (10ths of a millimeter), Speed={Speed} (100/RPM)" ,
                        Height ,
                        Speed ) ;
    }
}