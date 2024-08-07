﻿using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Linak.Tests
{
    [ TestClass ]
    public class DeskMoverFactoryTests
    {
        [ TestInitialize ]
        public void Initialize ( )
        {
            _factory = TestFactory ;

            _executor       = Substitute.For < IDeskCommandExecutor > ( ) ;
            _heightAndSpeed = Substitute.For < IDeskHeightAndSpeed > ( ) ;
        }

        private IDeskMover TestFactory ( IDeskCommandExecutor executor ,
                                         IDeskHeightAndSpeed  heightAndSpeed )
        {
            return Substitute.For < IDeskMover > ( ) ;
        }

        [ TestMethod ]
        public void Create_ForExecutorNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).Create ( null! ,
                                                    _heightAndSpeed ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "executor" ) ;
        }

        [ TestMethod ]
        public void CreateForInvoked_ReturnsInstance ( )
        {
            CreateSut ( ).Create ( _executor ,
                                   _heightAndSpeed )
                         .Should ( )
                         .NotBeNull ( ) ;
        }

        private DeskMoverFactory CreateSut ( )
        {
            return new DeskMoverFactory ( _factory ) ;
        }

        private IDeskCommandExecutor _executor       = null! ;
        private DeskMover.Factory    _factory        = null! ;
        private IDeskHeightAndSpeed  _heightAndSpeed = null! ;
    }
}