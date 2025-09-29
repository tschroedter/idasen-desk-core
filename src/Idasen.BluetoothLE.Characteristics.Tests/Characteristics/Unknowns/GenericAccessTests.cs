using FluentAssertions;
using Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;
using Idasen.BluetoothLE.Characteristics.Common;

namespace Idasen.BluetoothLE.Characteristics.Tests.Characteristics.Unknowns;

[TestClass]
public class GenericAccessTests
{
    [TestMethod]
    public void GattServiceUuid_ForInvoked_Empty()
    {
        using var sut = CreateSut();

        sut.GattServiceUuid
            .Should()
            .Be(Guid.Empty);
    }

    [TestMethod]
    public void RawResolution_ForInvoked_Empty()
    {
        using var sut = CreateSut();

        sut.RawResolution
            .Should()
            .BeEmpty();
    }

    [TestMethod]
    public void RawParameters_ForInvoked_Empty()
    {
        using var sut = CreateSut();

        sut.RawParameters
            .Should()
            .BeEmpty();
    }

    [TestMethod]
    public void RawAppearance_ForInvoked_Empty()
    {
        using var sut = CreateSut();

        sut.RawAppearance
            .Should()
            .BeEmpty();
    }

    [TestMethod]
    public void RawDeviceName_ForInvoked_Empty()
    {
        using var sut = CreateSut();

        sut.RawDeviceName
            .Should()
            .BeEmpty();
    }

    [TestMethod]
    public void AppearanceChanged_ForInvoked_Throws()
    {
        Action action = () => CreateSut().AppearanceChanged
            .Subscribe();

        action.Should()
            .Throw<NotInitializeException>();
    }

    [TestMethod]
    public void ParametersChanged_ForInvoked_Throws()
    {
        Action action = () => CreateSut().ParametersChanged
            .Subscribe();

        action.Should()
            .Throw<NotInitializeException>();
    }

    [TestMethod]
    public void ResolutionChanged_ForInvoked_Throws()
    {
        Action action = () => CreateSut().ResolutionChanged
            .Subscribe();

        action.Should()
            .Throw<NotInitializeException>();
    }

    [TestMethod]
    public void DeviceNameChanged_ForInvoked_Throws()
    {
        Action action = () => CreateSut().DeviceNameChanged
            .Subscribe();

        action.Should()
            .Throw<NotInitializeException>();
    }

    private static GenericAccess CreateSut() => new();
}
