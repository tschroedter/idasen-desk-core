using Windows.Storage.Streams;
using FluentAssertions;
using Idasen.BluetoothLE.Characteristics.Common;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using NSubstitute;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common;

[TestClass]
public class RawValueHandlerTests
{
    private IRawValueReader _valueReader     = null!;
    private IRawValueWriter _valueWriter     = null!;
    private RawValueHandler _rawValueHandler = null!;

    [TestInitialize]
    public void Initialize()
    {
        _valueReader     = Substitute.For<IRawValueReader>();
        _valueWriter     = Substitute.For<IRawValueWriter>();
        _rawValueHandler = new RawValueHandler(_valueReader, _valueWriter);
    }

    [TestMethod]
    public async Task TryReadValueAsync_ShouldCallValueReader()
    {
        var characteristic = Substitute.For<IGattCharacteristicWrapper>();
        _valueReader.TryReadValueAsync(characteristic).Returns(Task.FromResult((true, new byte[] { 1, 2, 3 })));

        var result = await _rawValueHandler.TryReadValueAsync(characteristic);

        result.Item1.Should().BeTrue();
        result.Item2.Should().BeEquivalentTo(new byte[] { 1, 2, 3 });
        await _valueReader.Received(1)
                          .TryReadValueAsync(characteristic);
    }

    [TestMethod]
    public async Task TryWriteValueAsync_ShouldCallValueWriter()
    {
        var characteristic = Substitute.For<IGattCharacteristicWrapper>();
        var buffer         = Substitute.For<IBuffer>();
        _valueWriter.TryWriteValueAsync(characteristic, buffer).Returns(Task.FromResult(true));

        var result = await _rawValueHandler.TryWriteValueAsync(characteristic, buffer);

        result.Should().BeTrue();
        await _valueWriter.Received(1).TryWriteValueAsync(characteristic, buffer);
    }

    [TestMethod]
    public async Task TryWritableAuxiliariesValueAsync_ShouldCallValueWriter()
    {
        var characteristic = Substitute.For<IGattCharacteristicWrapper>();
        var buffer         = Substitute.For<IBuffer>();
        _valueWriter.TryWritableAuxiliariesValueAsync(characteristic, buffer).Returns(Task.FromResult(true));

        var result = await _rawValueHandler.TryWritableAuxiliariesValueAsync(characteristic, buffer);

        result.Should().BeTrue();
        await _valueWriter.Received(1).TryWritableAuxiliariesValueAsync(characteristic, buffer);
    }

    [TestMethod]
    public async Task TryWriteWithoutResponseAsync_ShouldCallValueWriter()
    {
        var characteristic = Substitute.For<IGattCharacteristicWrapper>();
        var buffer         = Substitute.For<IBuffer>();
        var writeResult    = Substitute.For<IGattWriteResultWrapper>();
        _valueWriter.TryWriteWithoutResponseAsync(characteristic, buffer).Returns(Task.FromResult(writeResult));

        var result = await _rawValueHandler.TryWriteWithoutResponseAsync(characteristic, buffer);

        result.Should().Be(writeResult);
        await _valueWriter.Received(1).TryWriteWithoutResponseAsync(characteristic, buffer);
    }
}
