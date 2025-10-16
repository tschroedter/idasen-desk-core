using FluentAssertions ;
using Idasen.BluetoothLE.Characteristics.Characteristics ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics ;

[ TestClass ]
public class ReferenceInputTest
    : CharacteristicBaseTests < ReferenceInput >
{
    [ TestMethod ]
    public void RawDpg_ForNotRefreshedAndInvoked_EmptyBytes ( )
    {
        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        sut.Initialize < ReferenceInput > ( ) ;

        sut.Ctrl1
           .Should ( )
           .BeEquivalentTo ( CharacteristicBase.RawArrayEmpty ) ;
    }

    [ TestMethod ]
    public async Task RawDpg_ForRefreshedAndInvoked_Bytes ( )
    {
        using var sut = CreateSut ( ) ;

        ServiceWrapper.Uuid
                      .Returns ( sut.GattServiceUuid ) ;

        await sut.Initialize < ReferenceInput > ( )
                 .Refresh ( ) ;

        sut.Ctrl1
           .Should ( )
           .BeEquivalentTo ( RawValue1 ) ;
    }

    protected override ReferenceInput CreateSut ( )
    {
        return new ReferenceInput ( Logger ,
                                    Scheduler ,
                                    Device ,
                                    ProviderFactory ,
                                    RawValueHandler,
                                    ToStringConverter ,
                                    DescriptionToUuid ) ;
    }

    protected override void PopulateWrappers ( )
    {
        Wrappers.Add ( ReferenceInput.Ctrl1Key ,
                       CharacteristicWrapper1 ) ;
    }
}
