namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

using BluetoothLE.Characteristics.Characteristics ;
using BluetoothLE.Characteristics.Common ;
using FluentAssertions ;
using NSubstitute ;

[ TestClass ]
public class GenericAttributeTest
    : CharacteristicBaseTests < GenericAttribute >
{
    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        GenericAttribute sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        Action action = ( ) => sut.Initialize < Dpg > ( ) ;

        action.Should ( )
              .Throw < Exception > ( ) ;
    }

    [ TestMethod ]
    public async Task RawDpg_ForRefreshedAndInvoked_Bytes ( )
    {
        GenericAttribute sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < GenericAttribute > ( )
                 .Refresh ( ) ;

        sut.RawServiceChanged
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    protected override GenericAttribute CreateSut ( )
    {
        return new GenericAttribute ( Logger ,
                                      Scheduler ,
                                      Device ,
                                      ProviderFactory ,
                                      RawValueReader ,
                                      RawValueWriter ,
                                      ToStringConverter ,
                                      DescriptionToUuid ,
                                      new AllGattCharacteristicsProvider ( ) ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( GenericAttribute.CharacteristicServiceChanged ,
                       CharacteristicWrapper1 ) ;
    }
}
