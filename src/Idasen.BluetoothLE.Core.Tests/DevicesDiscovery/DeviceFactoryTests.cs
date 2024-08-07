﻿using FluentAssertions ;
using FluentAssertions.Execution ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Idasen.BluetoothLE.Core.Interfaces ;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery
{
    [ TestClass ]
    public class DeviceFactoryTests
    {
        [ TestInitialize ]
        public void Initialize ( )
        {
            _factory = TestFactory ;

            _broadcastTime          = new DateTimeOffsetWrapper ( ).Now ;
            _address                = 254682828386071 ;
            _name                   = "Name" ;
            _rawSignalStrengthInDBm = - 50 ;
        }

        private IDevice TestFactory ( IDateTimeOffset broadcastTime ,
                                      ulong           address ,
                                      string ?        name ,
                                      short           rawSignalStrengthInDBm )
        {
            return new Device ( broadcastTime ,
                                address ,
                                name ,
                                rawSignalStrengthInDBm ) ;
        }

        [ TestMethod ]
        public void Create_ForBroadcastTimesNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).Create ( null! ,
                                                    _address ,
                                                    _name ,
                                                    _rawSignalStrengthInDBm ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "broadcastTime" ) ;
        }

        [ TestMethod ]
        public void CreateForInvoked_ReturnsInstance ( )
        {
            var actual = CreateSut ( ).Create ( _broadcastTime ,
                                                _address ,
                                                _name ,
                                                _rawSignalStrengthInDBm ) ;

            using ( new AssertionScope ( ) )
            {
                actual.BroadcastTime
                      .Should ( )
                      .Be ( _broadcastTime ) ;

                actual.Address
                      .Should ( )
                      .Be ( _address ) ;

                actual.Name
                      .Should ( )
                      .Be ( _name ) ;

                actual.RawSignalStrengthInDBm
                      .Should ( )
                      .Be ( _rawSignalStrengthInDBm ) ;
            }
        }

        private DeviceFactory CreateSut ( )
        {
            return new DeviceFactory ( _factory ) ;
        }

        private ulong           _address ;
        private IDateTimeOffset _broadcastTime = null! ;
        private Device.Factory  _factory       = null! ;
        private string          _name          = null! ;
        private short           _rawSignalStrengthInDBm ;
    }
}