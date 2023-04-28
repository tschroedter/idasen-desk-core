using System ;
using System.Reactive.Subjects ;
using Idasen.BluetoothLE.Linak.Control ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using Microsoft.Reactive.Testing ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;
using NSubstitute ;
using Serilog ;

namespace Idasen.BluetoothLE.Linak.Tests
{
    [ TestClass ]
    public class DeskLockerTests
    {
        [ TestInitialize ]
        public void Initialize ( )
        {
            _logger         = Substitute.For < ILogger > ( ) ;
            _scheduler      = new TestScheduler ( ) ;
            _deskMover      = Substitute.For < IDeskMover > ( ) ;
            _executer       = Substitute.For < IDeskCommandExecutor > ( ) ;
            _heightAndSpeed = Substitute.For < IDeskHeightAndSpeed > ( ) ;

            _subjectHeightAndSpeed = new Subject < HeightSpeedDetails > ( ) ;

            _heightAndSpeed.HeightAndSpeedChanged
                           .Returns ( _subjectHeightAndSpeed ) ;

            _details = new HeightSpeedDetails ( DateTimeOffset.Now ,
                                                123u ,
                                                321 ) ;
        }

        [ TestMethod ]
        public void OnHeightAndSpeedChanged_LockedAndIsAllowedToMoveTrue_DoesNotCallStop ( )
        {
            _deskMover.IsAllowedToMove
                      .Returns ( true ) ;

            var sut = CreateSutInitialized ( ) ;

            sut.Lock ( ) ;

            _subjectHeightAndSpeed.OnNext ( _details ) ;

            _scheduler.Start ( ) ;

            _executer.DidNotReceive ( )
                     .Stop ( ) ;
        }

        [ TestMethod ]
        public void OnHeightAndSpeedChanged_UnlockedAndIsAllowedToMoveTrue_DoesNotCallStop ( )
        {
            _deskMover.IsAllowedToMove
                      .Returns ( true ) ;

            var _ = CreateSutInitialized ( ).Unlock ( ) ;

            _subjectHeightAndSpeed.OnNext ( _details ) ;

            _scheduler.Start ( ) ;

            _executer.DidNotReceive ( )
                     .Stop ( ) ;
        }

        [ TestMethod ]
        public void OnHeightAndSpeedChanged_LockedAndIsAllowedToMoveIsTrue_DoesNotCallStop ( )
        {
            _deskMover.IsAllowedToMove
                      .Returns ( true ) ;

            var _ = CreateSutInitialized ( ).Lock ( ) ;

            _subjectHeightAndSpeed.OnNext ( _details ) ;

            _scheduler.Start ( ) ;

            _executer.DidNotReceive ( )
                     .Stop ( ) ;
        }

        [ TestMethod ]
        public void OnHeightAndSpeedChanged_LockedAndIsAllowedToMoveIsFalse_CallsStop ( )
        {
            _deskMover.IsAllowedToMove
                      .Returns ( false ) ;

            var _ = CreateSutInitialized ( ).Lock ( ) ;

            _subjectHeightAndSpeed.OnNext ( _details ) ;

            _scheduler.Start ( ) ;

            _executer.Received ( )
                     .Stop ( ) ;
        }

        private DeskLocker CreateSut ( )
        {
            return new DeskLocker ( _logger ,
                                    _scheduler ,
                                    _deskMover ,
                                    _executer ,
                                    _heightAndSpeed ) ;
        }

        private IDeskLocker CreateSutInitialized ( )
        {
            return CreateSut ( ).Initialize ( ) ;
        }

        private IDeskMover                     _deskMover             = null! ;
        private HeightSpeedDetails             _details               = null! ;
        private IDeskCommandExecutor           _executer              = null! ;
        private IDeskHeightAndSpeed            _heightAndSpeed        = null! ;
        private ILogger                        _logger                = null! ;
        private TestScheduler                  _scheduler             = null! ;
        private Subject < HeightSpeedDetails > _subjectHeightAndSpeed = null! ;
    }
}