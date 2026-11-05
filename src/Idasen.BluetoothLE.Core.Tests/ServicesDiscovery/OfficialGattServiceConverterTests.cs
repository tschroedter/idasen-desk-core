using CsvHelper;
using FluentAssertions;
using Idasen.BluetoothLE.Core.ServicesDiscovery;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery;

[TestClass]
public class OfficialGattServiceConverterTests
{
    private const string NotANumber = "not a number";

    private CsvReader _csvReader = null!;
    private CsvWriter _csvWriter = null!;
    private StringWriter _stringWriter = null!;

    [TestInitialize]
    public void Initialize()
    {
        _csvReader = CsvReader.Create(new StringReader("1800"));
        _stringWriter = new StringWriter();
        _csvWriter = CsvWriter.Create(_stringWriter);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _csvReader.Dispose();
        _csvWriter.Dispose();
        _stringWriter.Dispose();
    }

    [TestMethod]
    public void Read_ForTextNull_Zero()
    {
        // Setup reader position or pass index directly based on how it's consumed
        var reader = CsvReader.Create(new StringReader(","));
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
        var reader = CsvReader.Create(new StringReader("1800"), options);
        reader.Read();

        CreateSut().Read(reader, 0)
                   .Should()
                   .Be((ushort)0x1800);
    }

    [TestMethod]
    public void Read_ForTextIsNotANumber_MaxValue()
    {
        var options = new CsvReaderOptions { HasHeader = false };
        var reader = CsvReader.Create(new StringReader(NotANumber), options);
        reader.Read();

        CreateSut().Read(reader, 0)
                   .Should()
                   .Be(ushort.MaxValue);
    }

    [TestMethod]
    public void Write_ForValidValue_WritesField()
    {
        var sut = CreateSut();
        sut.Write(_csvWriter, 6144);
        _csvWriter.Flush();

        _stringWriter.ToString()
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
