using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns
{
    [ TestClass ]
    public class ControlTests
    {
        [ TestMethod ]
        public void RawControl2_ForInvoked_Empty ( )
        {
            CreateSut ( ).RawControl2
                         .Should ( )
                         .BeEmpty ( ) ;
        }

        [ TestMethod ]
        public void RawControl3_ForInvoked_Empty ( )
        {
            CreateSut ( ).RawControl3
                         .Should ( )
                         .BeEmpty ( ) ;
        }

        [ TestMethod ]
        public async Task TryWriteRawControl2_ForInvoked_ReturnsFalse ( )
        {
            var result = await CreateSut ( ).TryWriteRawControl2 ( [] ) ;

            result.Should ( )
                  .BeFalse ( ) ;
        }

        private Control CreateSut ( )
        {
            return new Control ( ) ;
        }
    }
}