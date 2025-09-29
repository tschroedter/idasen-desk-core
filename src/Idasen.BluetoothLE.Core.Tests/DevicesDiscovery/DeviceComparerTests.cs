using FluentAssertions;
using Idasen.BluetoothLE.Core.DevicesDiscovery;
using Idasen.BluetoothLE.Core.Interfaces.DevicesDiscovery;
using NSubstitute;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery;

[TestClass]
public class DeviceComparerTests
{
    private readonly IDevice _deviceA = Substitute.For<IDevice>();
    private readonly IDevice _deviceB = Substitute.For<IDevice>();

    [TestMethod]
    public void IsEquivalentTo_ForBothNull_ReturnsFalse()
    {
        CreateSut().IsEquivalentTo(
                null,
                null)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void IsEquivalentTo_ForDeviceAIsNull_ReturnsFalse()
    {
        CreateSut().IsEquivalentTo(
                null,
                _deviceA)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void IsEquivalentTo_ForDeviceBIsNull_ReturnsFalse()
    {
        CreateSut().IsEquivalentTo(
                _deviceA,
                null)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void IsEquivalentTo_ForSameDevice_ReturnsTrue()
    {
        CreateSut().IsEquivalentTo(
                _deviceA,
                _deviceB)
            .Should()
            .BeTrue();
    }

    [TestMethod]
    public void IsEquivalentTo_For2DeviceDifferentName_ReturnsTrue()
    {
        _deviceB.Address.Returns(_deviceA.Address);
        _deviceB.Name.Returns("Other Name");
        _deviceB.RawSignalStrengthInDBm.Returns(_deviceA.RawSignalStrengthInDBm);

        CreateSut().IsEquivalentTo(
                _deviceA,
                _deviceB)
            .Should()
            .BeTrue();
    }

    [TestMethod]
    public void IsEquivalentTo_For2DeviceDifferentBroadcastTime_ReturnsTrue()
    {
        _deviceB.Address.Returns(_deviceA.Address);
        _deviceB.Name.Returns(_deviceA.Name);
        _deviceB.RawSignalStrengthInDBm.Returns(_deviceA.RawSignalStrengthInDBm);

        CreateSut().IsEquivalentTo(
                _deviceA,
                _deviceB)
            .Should()
            .BeTrue();
    }

    [TestMethod]
    public void IsEquivalentTo_For2DeviceRawSignalStrengthInDBm_ReturnsTrue()
    {
        _deviceA.RawSignalStrengthInDBm
            .Returns((short)-1);

        var deviceB = new Device(
            _deviceA.BroadcastTime,
            _deviceA.Address,
            _deviceA.Name,
            0);

        CreateSut().IsEquivalentTo(
                _deviceA,
                deviceB)
            .Should()
            .BeTrue();
    }

    [TestMethod]
    public void IsEquivalentTo_For2DevicesDifferentAddress_ReturnsFalse()
    {
        _deviceA.Address
            .Returns(0ul);

        var deviceB = new Device(
            _deviceA.BroadcastTime,
            1ul,
            _deviceA.Name,
            _deviceA.RawSignalStrengthInDBm);

        CreateSut().IsEquivalentTo(
                _deviceA,
                deviceB)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_ForBothNull_ReturnsFalse()
    {
        CreateSut().Equals(
                null,
                null)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_ForDeviceAIsNull_ReturnsFalse()
    {
        CreateSut().Equals(
                null,
                _deviceA)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_ForDeviceBIsNull_ReturnsFalse()
    {
        CreateSut().Equals(
                _deviceA,
                null)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_ForTheSameDevice_ReturnsTrue()
    {
        CreateSut().Equals(
                _deviceA,
                _deviceA)
            .Should()
            .BeTrue();
    }

    [TestMethod]
    public void Equals_For2DeviceDifferentName_ReturnsFalse()
    {
        _deviceB.BroadcastTime.Returns(_deviceA.BroadcastTime);
        _deviceB.Address.Returns(_deviceA.Address);
        _deviceB.Name.Returns("Other Name");
        _deviceB.RawSignalStrengthInDBm.Returns(_deviceA.RawSignalStrengthInDBm);

        CreateSut().Equals(
                _deviceA,
                _deviceB)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_For2DeviceDifferentBroadcastTime_ReturnsTrue()
    {
        _deviceB.Address.Returns(_deviceA.Address);
        _deviceB.Name.Returns(_deviceA.Name);
        _deviceB.RawSignalStrengthInDBm.Returns(_deviceA.RawSignalStrengthInDBm);

        CreateSut().Equals(
                _deviceA,
                _deviceB)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_For2DeviceRawSignalStrengthInDBm_ReturnsTrue()
    {
        _deviceA.RawSignalStrengthInDBm
            .Returns((short)-1);

        _deviceB.BroadcastTime.Returns(_deviceA.BroadcastTime);
        _deviceB.Address.Returns(_deviceA.Address);
        _deviceB.Name.Returns(_deviceA.Name);

        CreateSut().Equals(
                _deviceA,
                _deviceB)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void Equals_For2DevicesDifferentAddress_ReturnsFalse()
    {
        _deviceA.Address
            .Returns(0ul);

        _deviceB.BroadcastTime.Returns(_deviceA.BroadcastTime);
        _deviceB.Address.Returns(1ul);
        _deviceB.Name.Returns(_deviceA.Name);
        _deviceB.RawSignalStrengthInDBm.Returns(_deviceA.RawSignalStrengthInDBm);

        CreateSut().Equals(
                _deviceA,
                _deviceB)
            .Should()
            .BeFalse();
    }

    private static DeviceComparer CreateSut() => new();
}
