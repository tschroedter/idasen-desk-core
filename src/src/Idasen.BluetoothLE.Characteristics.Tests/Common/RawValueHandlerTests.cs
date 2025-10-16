using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Idasen.BluetoothLE.Characteristics.Common;
using Idasen.BluetoothLE.Characteristics.Interfaces.Common;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using NSubstitute;
using NUnit.Framework;

namespace Idasen.BluetoothLE.Characteristics.Tests.Common
{
    [TestFixture]
    public class RawValueHandlerTests
    {
        private IRawValueReader _valueReader;
        private IRawValueWriter _valueWriter;
        private RawValueHandler _rawValueHandler;

        [SetUp]
        public void SetUp()
        {
            _valueReader = Substitute.For<IRawValueReader>();
            _valueWriter = Substitute.For<IRawValueWriter>();
            _rawValueHandler = new RawValueHandler(_valueReader, _valueWriter);
        }

        [Test]
        public async Task TryReadValueAsync_ShouldCallValueReader()
        {
            var characteristic = Substitute.For<IGattCharacteristicWrapper>();
            _valueReader.TryReadValueAsync(characteristic).Returns(Task.FromResult((true, new byte[] { 1, 2, 3 })));

            var result = await _rawValueHandler.TryReadValueAsync(characteristic);

            Assert.IsTrue(result.Item1);
            Assert.AreEqual(new byte[] { 1, 2, 3 }, result.Item2);
            await _valueReader.Received(1).TryReadValueAsync(characteristic);
        }

        [Test]
        public async Task TryWriteValueAsync_ShouldCallValueWriter()
        {
            var characteristic = Substitute.For<IGattCharacteristicWrapper>();
            var buffer = Substitute.For<IBuffer>();
            _valueWriter.TryWriteValueAsync(characteristic, buffer).Returns(Task.FromResult(true));

            var result = await _rawValueHandler.TryWriteValueAsync(characteristic, buffer);

            Assert.IsTrue(result);
            await _valueWriter.Received(1).TryWriteValueAsync(characteristic, buffer);
        }

        [Test]
        public async Task TryWritableAuxiliariesValueAsync_ShouldCallValueWriter()
        {
            var characteristic = Substitute.For<IGattCharacteristicWrapper>();
            var buffer = Substitute.For<IBuffer>();
            _valueWriter.TryWritableAuxiliariesValueAsync(characteristic, buffer).Returns(Task.FromResult(true));

            var result = await _rawValueHandler.TryWritableAuxiliariesValueAsync(characteristic, buffer);

            Assert.IsTrue(result);
            await _valueWriter.Received(1).TryWritableAuxiliariesValueAsync(characteristic, buffer);
        }

        [Test]
        public async Task TryWriteWithoutResponseAsync_ShouldCallValueWriter()
        {
            var characteristic = Substitute.For<IGattCharacteristicWrapper>();
            var buffer = Substitute.For<IBuffer>();
            var writeResult = Substitute.For<IGattWriteResultWrapper>();
            _valueWriter.TryWriteWithoutResponseAsync(characteristic, buffer).Returns(Task.FromResult(writeResult));

            var result = await _rawValueHandler.TryWriteWithoutResponseAsync(characteristic, buffer);

            Assert.AreEqual(writeResult, result);
            await _valueWriter.Received(1).TryWriteWithoutResponseAsync(characteristic, buffer);
        }
    }
}
