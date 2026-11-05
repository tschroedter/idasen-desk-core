using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core ;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common.ConstructorNullTester ;

[ TestClass ]
public class ResourceNotFoundExceptionConstructorNullTests
    : BaseConstructorNullTester < ResourceNotFoundException >
{
    public override int NumberOfConstructorsPassed => 1 ;
    public override int NumberOfConstructorsFailed => 0 ;

    [ TestMethod ]
    public override void Constructor_ForAnyParameterNullThrows_AllPassing ( )
    {
        Action nullResourceName = ( ) => _ = new ResourceNotFoundException ( null! ,
                                                                              "Message" ) ;
        nullResourceName.Should ( )
                        .Throw < ArgumentNullException > ( )
                        .WithParameter ( "resourceName" ) ;

        var exception = new ResourceNotFoundException ( "ResourceName" ,
                                                        null ) ;
        exception.Message.Should ( )
                          .Be ( "Exception of type 'Idasen.BluetoothLE.Core.ResourceNotFoundException' was thrown." ) ;
    }
}
