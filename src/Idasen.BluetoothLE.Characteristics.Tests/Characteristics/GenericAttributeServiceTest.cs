using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using Idasen.BluetoothLE.Characteristics.Common ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

[ TestClass ]
public class GenericAttributeServiceTest
    : CharacteristicBaseTests < GenericAttributeService >
{
    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        // ReSharper disable once AccessToDisposedClosure
        Action action = ( ) => sut.Initialize < Dpg > ( ) ;

        action.Should ( )
              .Throw < Exception > ( ) ;
    }

    [ TestMethod ]
    public async Task RawDpg_ForRefreshedAndInvoked_Bytes ( )
    {
        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < GenericAttributeService > ( )
                 .Refresh ( ) ;

        sut.RawServiceChanged
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    protected override GenericAttributeService CreateSut ( )
    {
        return new GenericAttributeService ( Logger ,
                                             Scheduler ,
                                             Device ,
                                             ProviderFactory ,
                                             RawValueHandler ,
                                             ToStringConverter ,
                                             DescriptionToUuid ,
                                             new AllGattCharacteristicsProvider ( ) ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( GenericAttributeService.CharacteristicServiceChanged ,
                       CharacteristicWrapper1 ) ;
    }
}