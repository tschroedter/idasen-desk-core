using System.Diagnostics.CodeAnalysis ;
using System.Text ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

/// <inheritdoc />
[ ExcludeFromCodeCoverage ]
[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicsResultWrapper
    : IGattCharacteristicsResultWrapper
{
    public delegate IGattCharacteristicsResultWrapper Factory ( GattCharacteristicsResult result ) ;

    private readonly IGattCharacteristicWrapperFactory _factory ;

    private readonly ILogger                   _logger ;
    private readonly GattCharacteristicsResult _result ;
    private          bool                      _disposed ;

    public GattCharacteristicsResultWrapper ( ILogger                           logger ,
                                              IGattCharacteristicWrapperFactory factory ,
                                              GattCharacteristicsResult         result )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( factory ,
                                nameof ( factory ) ) ;
        Guard.ArgumentNotNull ( result ,
                                nameof ( result ) ) ;

        _logger  = logger ;
        _factory = factory ;
        _result  = result ;
    }

    /// <inheritdoc />
    public GattCommunicationStatus Status => _result.Status ;

    /// <inheritdoc />
    public byte ? ProtocolError => _result.ProtocolError ;

    /// <inheritdoc />
    public IReadOnlyList < IGattCharacteristicWrapper > Characteristics { get ; private set ; } =
        new List < IGattCharacteristicWrapper > ( ) ;

    public async Task < IGattCharacteristicsResultWrapper > Initialize ( )
    {
        // Safeguard against unsuccessful status or missing characteristics
        if ( _result.Status != GattCommunicationStatus.Success ||
             _result.Characteristics is null                   ||
             _result.Characteristics.Count == 0 )
        {
            Characteristics = [] ;
            return this ;
        }

        var wrappers = _result.Characteristics
                              .Select ( characteristic => _factory.Create ( characteristic ) )
                              .ToList ( ) ;

        // Initialize all wrappers in parallel; continue on failure of any item
        IEnumerable < Task < IGattCharacteristicWrapper ? > > initTasks = wrappers.Select ( async w =>
        {
            try
            {
                await w.Initialize ( ) ;
                return w ;
            }
            catch ( Exception ex )
            {
                try
                {
                    _logger.Warning ( ex ,
                                      "Failed to initialize GATT characteristic wrapper {Uuid}" ,
                                      w.Uuid ) ;
                }
                catch ( Exception e )
                {
                    _logger.Warning ( e,
                                      "Failed to initialize a GATT characteristic wrapper" ) ;
                }

                // swallow to continue; problematic wrapper is excluded
                w.Dispose ( ) ;
                return null ;
            }
        } ) ;

        var initialized = await Task.WhenAll ( initTasks ) ;

        Characteristics = initialized.Where ( w => w is not null ).ToList ( )! ;

        return this ;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing) {
            // Dispose individual characteristic wrappers to release event subscriptions
            if (Characteristics is { Count: > 0 })
                foreach (var c in Characteristics)
                    c.Dispose();

            Characteristics = [];
        }

        _disposed = true;
    }

    public override string ToString ( )
    {
        var builder = new StringBuilder ( ) ;

        builder.Append ( CharacteristicsToString ( ) ) ;

        return builder.ToString ( ) ;
    }

    private string CharacteristicsToString ( )
    {
        var list = new List < string > ( ) ;

        foreach ( var characteristic in Characteristics )
        {
            var properties = characteristic.CharacteristicProperties.ToCsv ( ) ;
            var formats    = PresentationFormatsToString ( characteristic.PresentationFormats ) ;

            list.Add ( $"Service UUID = {characteristic.ServiceUuid} "         +
                       $"Characteristic UUID = {characteristic.Uuid}, "        +
                       $"UserDescription = {characteristic.UserDescription}, " +
                       $"ProtectionLevel = {characteristic.ProtectionLevel}, " +
                       $"AttributeHandle = {characteristic.AttributeHandle}, " +
                       $"CharacteristicProperties = [{properties}] "           +
                       $"PresentationFormats = [{formats}]" ) ;
        }

        return string.Join ( Environment.NewLine ,
                             list ) ;
    }

    private static string PresentationFormatsToString (
        IReadOnlyList < GattPresentationFormat > characteristicPresentationFormats )
    {
        var list = new List < string > ( ) ;

        foreach ( var format in characteristicPresentationFormats )
            list.Add ( "GattPresentationFormat - "             +
                       $"Description = {format.Description}, " +
                       $"FormatType = {format.FormatType}, "   +
                       $"Namespace = {format.Namespace}, "     +
                       $"Exponent = {format.Exponent}, "       +
                       $"Unit = {format.Unit}" ) ;

        return string.Join ( ", " ,
                             list ) ;
    }
}
