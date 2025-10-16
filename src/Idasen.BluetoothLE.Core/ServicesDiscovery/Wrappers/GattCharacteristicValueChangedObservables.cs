using System.Diagnostics.CodeAnalysis ;
using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using System.Reactive.Subjects ;
using System.Runtime.InteropServices.WindowsRuntime ;
using Windows.Devices.Bluetooth.GenericAttributeProfile ;
using Windows.Foundation ;
using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers ;
using Serilog ;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class GattCharacteristicValueChangedObservables
    : IGattCharacteristicValueChangedObservables
{
    private readonly ILogger                                            _logger ;
    private readonly IScheduler                                         _scheduler ;
    private readonly ISubject < GattCharacteristicValueChangedDetails > _subject ;
    private          IDisposable ?                                      _observable ;
    private          bool                                               _disposed ;

    public GattCharacteristicValueChangedObservables ( ILogger                                            logger ,
                                                       IScheduler                                         scheduler ,
                                                       ISubject < GattCharacteristicValueChangedDetails > subject )
    {
        Guard.ArgumentNotNull ( logger ,
                                nameof ( logger ) ) ;
        Guard.ArgumentNotNull ( scheduler ,
                                nameof ( scheduler ) ) ;
        Guard.ArgumentNotNull ( subject ,
                                nameof ( subject ) ) ;

        _logger    = logger ;
        _scheduler = scheduler ;
        _subject   = subject ;
    }

    public IObservable < GattCharacteristicValueChangedDetails > ValueChanged => _subject ;

    [ ExcludeFromCodeCoverage ]
    public async Task Initialise ( GattCharacteristic characteristic )
    {
        var properties  = characteristic.CharacteristicProperties ;
        var serviceUuid = characteristic.Service?.Uuid ?? Guid.Empty ;

        _logger.Debug ( "Service UUID = {ServiceUuid} Characteristic UUID = {CharacteristicUuid} Notify = {Notify} Indicate = {Indicate} Write = {Write} WriteWithoutResponse = {WriteWithoutResponse}" ,
                        serviceUuid ,
                        characteristic.Uuid ,
                        properties.HasFlag ( GattCharacteristicProperties.Notify ) ,
                        properties.HasFlag ( GattCharacteristicProperties.Indicate ) ,
                        properties.HasFlag ( GattCharacteristicProperties.Write ) ,
                        properties.HasFlag ( GattCharacteristicProperties.WriteWithoutResponse ) ) ;

        if ( properties.HasFlag ( GattCharacteristicProperties.Notify ) ||
             properties.HasFlag ( GattCharacteristicProperties.Indicate ) ) /*&&
            (properties.HasFlag(GattCharacteristicProperties.Write) ||
             properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)))*/
        {
            var value = properties.HasFlag ( GattCharacteristicProperties.Notify )
                            ? GattClientCharacteristicConfigurationDescriptorValue.Notify
                            : GattClientCharacteristicConfigurationDescriptorValue.Indicate ;

            try
            {
                var status =
                    await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync ( value ) ;

                if ( status == GattCommunicationStatus.Success )
                {
                    _logger.Debug ( "Notify/Indicate Service UUID = {ServiceUuid} Characteristic UUID = {CharacteristicUuid} - Subscribing to ValueChanged" ,
                                    serviceUuid ,
                                    characteristic.Uuid ) ;

                    // dispose any prior subscription to avoid duplicate handlers/leaks
                    DisposeSubscription ( ) ;

                    _observable = Observable.FromEventPattern < TypedEventHandler < GattCharacteristic , GattValueChangedEventArgs > ,
                                                 GattCharacteristic ,
                                                 GattValueChangedEventArgs > ( h => characteristic.ValueChanged += h ,
                                                                               h => characteristic.ValueChanged -= h )
                                            .SubscribeOn ( _scheduler )
                                            .Subscribe ( e => OnValueChanged ( e.Sender! ,
                                                                               e.EventArgs ,
                                                                               _subject ) ) ;
                }
                else
                {
                    throw new InvalidOperationException ( $"Service UUID = {serviceUuid} "                  +
                                                          $"Characteristic UUID = {characteristic.Uuid} - " +
                                                          "Failed to subscribe to ValueChanged" ) ;
                }

                var result =
                    await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync ( ) ;

                if ( result.Status == GattCommunicationStatus.Success )
                    _logger.Debug ( "{Status} {Descriptor}" ,
                                    result.Status ,
                                    result.ClientCharacteristicConfigurationDescriptor ) ;
            }
            catch (UnauthorizedAccessException e)
            {
                _logger.Warning ( e ,
                                  "Access denied - Bluetooth is probably disabled" ) ;
            }
            catch ( Exception e )
            {
                _logger.Error ( e ,
                                "Failed" ) ;
            }
        }
    }

    [ ExcludeFromCodeCoverage ]
    public void Dispose ( )
    {
        Dispose ( true ) ;
        GC.SuppressFinalize ( this ) ;
    }

    protected virtual void Dispose ( bool disposing )
    {
        if ( _disposed )
            return ;

        if ( disposing )
        {
            try
            {
                DisposeSubscription ( ) ;
            }
            catch
            {
                // S6667: Dispose must not throw, even if called multiple times or if _observable is already disposed
            }
        }

        _disposed = true ;
    }

    [ ExcludeFromCodeCoverage ]
    private void OnValueChanged ( GattCharacteristic                                 sender ,
                                  GattValueChangedEventArgs                          args ,
                                  ISubject < GattCharacteristicValueChangedDetails > subject )
    {
        try
        {
            var bytes = args.CharacteristicValue?.ToArray ( ) ?? [] ;

            var details = new GattCharacteristicValueChangedDetails ( sender.Uuid ,
                                                                      bytes ,
                                                                      args.Timestamp ) ;
            subject.OnNext ( details ) ;
        }
        catch ( Exception e )
        {
            _logger.Error ( e ,
                            "Failed to handle ValueChange event" ) ;
        }
    }

    [ ExcludeFromCodeCoverage ]
    private void DisposeSubscription ( )
    {
        try
        {
            _observable?.Dispose ( ) ;
        }
        catch
        {
            // S6667: Dispose must not throw
        }
        _observable = null ;
    }
}
