using FluentAssertions ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery ;

[ TestClass ]
public class OfficialGattServicesTests
{
    private readonly Guid _knownGuid = Guid.Parse ( "00001800-0000-1000-8000-00805f9b34fb" ) ;

    [ TestMethod ]
    public void Constructor_ForInvoke_SetsResourceName ( )
    {
        var expected = "Idasen.BluetoothLE.Core.ServicesDiscovery.OfficialGattServices.txt" ;

        CreateSut ( ).ResourceName
                     .Should ( )
                     .Be ( expected ) ;
    }

    [ TestMethod ]
    public void Count_ForInvoke_ReturnsCorrectNumber ( )
    {
        var expected = CreateUuidCollection ( ).Count ;

        CreateSut ( ).Count
                     .Should ( )
                     .Be ( expected ) ;
    }

    [ TestMethod ]
    public void TryFindByUuid_ForUnknownUuid_ReturnsFalse ( )
    {
        CreateSut ( ).TryFindByUuid ( Guid.Empty ,
                                      out _ )
                     .Should ( )
                     .BeFalse ( ) ;
    }

    [ TestMethod ]
    public void TryFindByUuid_ForUnknownUuid_ReturnsNull ( )
    {
        CreateSut ( ).TryFindByUuid ( Guid.Empty ,
                                      out var gattService ) ;

        gattService.Should ( )
                   .BeNull ( ) ;
    }

    [ TestMethod ]
    public void TryFindByUuid_ForKnownUuid_ReturnsTrue ( )
    {
        CreateSut ( ).TryFindByUuid ( _knownGuid ,
                                      out _ )
                     .Should ( )
                     .BeTrue ( ) ;
    }

    [ TestMethod ]
    public void TryFindByUuid_ForKnownUuid_ReturnsGattService ( )
    {
        CreateSut ( ).TryFindByUuid ( _knownGuid ,
                                      out var gattService ) ;

        gattService.Should ( )
                   .NotBeNull ( ) ;
    }

    [ TestMethod ]
    public void TryFindByUuid_ForAllKnownUUIDs_ReturnsGattService ( )
    {
        var sut = CreateSut ( ) ;

        foreach ( var assignedNumber in CreateUuidCollection ( ) )
        {
            var knownGuid = Guid.Parse ( "0000"                          +
                                         assignedNumber.ToString ( "X" ) +
                                         "-0000-1000-8000-00805f9b34fb" ) ;

            sut.TryFindByUuid ( knownGuid ,
                                out var gattService ) ;

            gattService.Should ( )
                       .NotBeNull ( ) ;
        }
    }

    [ TestMethod ]
    public void GetEnumerator_ForInvoke_ReturnsCollection ( )
    {
        var expected = CreateUuidCollection ( ).Count ;

        var count = CreateSut ( ).Count ;

        count.Should ( )
             .Be ( expected ) ;
    }

    private IReadOnlyCollection < ushort > CreateUuidCollection ( )
    {
        return
        [
            0x1800 ,
            0x1811 ,
            0x1815 ,
            0x180F ,
            0x183B ,
            0x1810 ,
            0x181B ,
            0x181E ,
            0x181F ,
            0x1805 ,
            0x1818 ,
            0x1816 ,
            0x180A ,
            0x183C ,
            0x181A ,
            0x1826 ,
            0x1801 ,
            0x1808 ,
            0x1809 ,
            0x180D ,
            0x1823 ,
            0x1812 ,
            0x1802 ,
            0x1821 ,
            0x183A ,
            0x1820 ,
            0x1803 ,
            0x1819 ,
            0x1827 ,
            0x1828 ,
            0x1807 ,
            0x1825 ,
            0x180E ,
            0x1822 ,
            0x1829 ,
            0x1806 ,
            0x1814 ,
            0x1813 ,
            0x1824 ,
            0x1804 ,
            0x181C ,
            0x181D
        ] ;
    }

    private static OfficialGattServices CreateSut ( )
    {
        return new OfficialGattServices ( ) ;
    }
}