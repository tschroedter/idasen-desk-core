using CsvHelper;
using FluentAssertions;
using Idasen.BluetoothLE.Core.ServicesDiscovery;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery;

[TestClass]
public class GattServiceCsvHelperMapTests
{
    [TestMethod]
    public void Read_ForValidRecord_MapsAllProperties()
    {
        const string csv = "Name,UniformTypeIdentifier,AssignedNumber,ProfileSpecification\n" +
                           "Test service,org.example.test_service,0x1234,Test specification";
        var options = new CsvReaderOptions { Separator = ',', HasHeader = true };
        using var reader = CsvReader.Create(new StringReader(csv), options);

        var service = reader.GetRecords<OfficialGattService>().Single();

        service.Name.Should().Be("Test service");
        service.UniformTypeIdentifier.Should().Be("org.example.test_service");
        service.AssignedNumber.Should().Be(0x1234);
        service.ProfileSpecification.Should().Be("Test specification");
    }

    [TestMethod]
    public void Properties_ForAssignedValues_ReturnValues()
    {
        var sut = new GattServiceCsvHelperMap
        {
            Name = "Test service",
            UniformTypeIdentifier = "org.example.test_service",
            AssignedNumber = 0x1234,
            ProfileSpecification = "Test specification"
        };

        sut.Name.Should().Be("Test service");
        sut.UniformTypeIdentifier.Should().Be("org.example.test_service");
        sut.AssignedNumber.Should().Be(0x1234);
        sut.ProfileSpecification.Should().Be("Test specification");
    }
}
