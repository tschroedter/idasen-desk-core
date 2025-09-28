namespace Idasen.BluetoothLE.Linak.Tests ;

using FluentAssertions ;
using Interfaces ;
using Linak.Control ;
using Selkie.AutoMocking ;

[ AutoDataTestClass ]
public class InitialHeightAndSpeedProviderFactoryTests
{
    [ AutoDataTestMethod ]
    public void Create_ForInvoked_Instance (
        InitialHeightAndSpeedProviderFactory sut ,
        IDeskCommandExecutor executor ,
        IDeskHeightAndSpeed heightAndSpeed )
    {
        sut.Create ( executor ,
                     heightAndSpeed )
           .Should ( )
           .NotBeNull ( ) ;
    }
}
