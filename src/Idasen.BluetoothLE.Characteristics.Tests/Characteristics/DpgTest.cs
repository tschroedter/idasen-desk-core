namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

using BluetoothLE.Characteristics.Characteristics ;
using FluentAssertions ;
using NSubstitute ;

[ TestClass ]
public class DpgTest
    : CharacteristicBaseTests < Dpg >
{
    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        Dpg sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < Dpg > ( ) ;

        sut.RawDpg
           .Should ( )
           .BeEquivalentTo ( CharacteristicBase.RawArrayEmpty ) ;
    }

    [ TestMethod ]
    public async Task RawDpg_ForRefreshedAndInvoked_Bytes ( )
    {
        Dpg sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < Dpg > ( )
                 .Refresh ( ) ;

        sut.RawDpg
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    protected override Dpg CreateSut ( )
    {
        return new Dpg ( Logger ,
                         Scheduler ,
                         Device ,
                         ProviderFactory ,
                         RawValueReader ,
                         RawValueWriter ,
                         ToStringConverter ,
                         DescriptionToUuid ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( Dpg.DpgKey ,
                       CharacteristicWrapper1 ) ;
    }
}
