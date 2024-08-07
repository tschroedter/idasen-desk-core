﻿using System.Diagnostics.CodeAnalysis ;
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

namespace Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class GattCharacteristicValueChangedObservables
        : IGattCharacteristicValueChangedObservables
    {
        public GattCharacteristicValueChangedObservables (
            ILogger                                            logger ,
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
            var properties = characteristic.CharacteristicProperties ;

            _logger.Information ( $"Service UUID = {characteristic.Service.Uuid} Characteristic UUID = {characteristic.Uuid} " +
                                  $"Notify = {properties.HasFlag ( GattCharacteristicProperties.Notify )} " +
                                  $"Indicate = {properties.HasFlag ( GattCharacteristicProperties.Indicate )} " +
                                  $"Write = {properties.HasFlag ( GattCharacteristicProperties.Write )} " +
                                  $"WriteWithoutResponse = {properties.HasFlag ( GattCharacteristicProperties.WriteWithoutResponse )}" ) ;

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
                        _logger.Information ( "Notify/Indicate "                                +
                                              $"Service UUID = {characteristic.Service.Uuid} "  +
                                              $"Characteristic UUID = {characteristic.Uuid} - " +
                                              "Subscribing to ValueChanged" ) ;

                        _observable = Observable
                                     .FromEventPattern
                                      < TypedEventHandler < GattCharacteristic , GattValueChangedEventArgs > ,
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
                        throw new Exception ( $"Service UUID = {characteristic.Service.Uuid} "  +
                                              $"Characteristic UUID = {characteristic.Uuid} - " +
                                              "Failed to subscribe to ValueChanged" ) ;
                    }

                    var result = await characteristic.ReadClientCharacteristicConfigurationDescriptorAsync ( ) ;

                    if ( result.Status == GattCommunicationStatus.Success )
                        _logger.Information ( $"{result.Status} {result.ClientCharacteristicConfigurationDescriptor}" ) ;
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
            DisposeSubscription ( ) ;
        }

        [ ExcludeFromCodeCoverage ]
        private void OnValueChanged (
            GattCharacteristic                                 sender ,
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
            _observable?.Dispose ( ) ;
        }

        private readonly ILogger                                            _logger ;
        private readonly IScheduler                                         _scheduler ;
        private readonly ISubject < GattCharacteristicValueChangedDetails > _subject ;
        private          IDisposable ?                                      _observable ;
    }
}