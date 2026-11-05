using CsvHelper;
using FluentAssertions;
using Idasen.BluetoothLE.Core.ServicesDiscovery;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery;

[TestClass]
public class OfficialGattServiceConverterTests
{
    private const string NotANumber = "not a number";

    [TestMethod]
    public void Read_ForTextNull_Zero()
    {
        using var reader = CsvReader.Create(new StringReader(","));
        reader.Read();

        CreateSut().Read(reader, 0)
                   .Should()
                   .Be(0);
    }

    [TestMethod]
    public void Read_ForReaderNull_Throws()
    {
        var action = () => {
            CreateSut().Read(null!, 0);
        };

        action.Should()
              .Throw<ArgumentNullException>();
    }

    [TestMethod]
    public void Read_ForTextIsNumber_Number()
    {
        var options = new CsvReaderOptions { HasHeader = false };
        using var reader = CsvReader.Create(new StringReader("1800"), options);
        reader.Read();

        CreateSut().Read(reader, 0)
                   .Should()
                   .Be((ushort)0x1800);
    }

    [TestMethod]
    public void Read_ForTextIsNotANumber_MaxValue()
    {
        var options = new CsvReaderOptions { HasHeader = false };
        using var reader = CsvReader.Create(new StringReader(NotANumber), options);
        reader.Read();

        CreateSut().Read(reader, 0)
                   .Should()
                   .Be(ushort.MaxValue);
    }

    [TestMethod]
    public void Write_ForValidValue_WritesField()
    {
        using var stringWriter = new StringWriter();
        using var csvWriter = CsvWriter.Create(stringWriter);
        var sut = CreateSut();
        sut.Write(csvWriter, 6144);
        csvWriter.Flush();

        stringWriter.ToString()
                     .Should()
                     .Be("6144");
    }

    [TestMethod]
    public void Write_ForWriterNull_Throws()
    {
        var action = () => {
            CreateSut().Write(null!, 6144);
        };

        action.Should()
              .Throw<ArgumentNullException>();
    }

    private static OfficialGattServiceConverter CreateSut()
    {
        return new OfficialGattServiceConverter();
    }
}
