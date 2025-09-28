namespace Idasen.BluetoothLE.Linak.Tests ;

using Characteristics.Interfaces.Characteristics ;
using Common.Tests ;
using FluentAssertions ;
using Interfaces ;
using NSubstitute ;

[ TestClass ]
public class DeskHeightAndSpeedFactoryTests
{
    private DeskHeightAndSpeed.Factory _factory = null! ;
    private IReferenceOutput _referenceOutput = null! ;

    [ TestInitialize ]
    public void Initialize ( )
    {
        _referenceOutput = Substitute.For < IReferenceOutput > ( ) ;
        _factory = TestFactory ;
    }

    [ TestMethod ]
    public void Create_ForReferenceOutputNull_Throws ( )
    {
        Action action = ( ) => { CreateSut ( ).Create ( null! ) ; } ;

        action.Should ( )
              .Throw < ArgumentNullException > ( )
              .WithParameter ( "referenceOutput" ) ;
    }

    [ TestMethod ]
    public void Create_ForInvoked_ReturnsInstance ( )
    {
        CreateSut ( ).Create ( _referenceOutput )
                     .Should ( )
                     .NotBeNull ( ) ;
    }

    private IDeskHeightAndSpeed TestFactory ( IReferenceOutput _ ) => Substitute.For < IDeskHeightAndSpeed > ( ) ;

    private DeskHeightAndSpeedFactory CreateSut ( ) => new ( _factory ) ;
}
