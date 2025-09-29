using FluentAssertions;
using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;
using Idasen.BluetoothLE.Core.ServicesDiscovery;
using NSubstitute;
using Selkie.AutoMocking;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery;

[AutoDataTestClass]
public class GattServicesTests
{
    [AutoDataTestMethod]
    public void Indexer_ForServiceAndResultNull_Throws(
        GattServices sut,
        IGattDeviceServiceWrapper service)
    {
        Action action = () => sut[service] = null!;

        action.Should()
            .Throw<ArgumentException>()
            .WithParameter("value");
    }

    [AutoDataTestMethod]
    public void Indexer_ForServiceAndResult_SetsKeyAndValue(
        GattServices sut,
        IGattDeviceServiceWrapper service,
        IGattCharacteristicsResultWrapper result)
    {
        sut[service] = result;

        sut[service]
            .Should()
            .Be(result);
    }

    [AutoDataTestMethod]
    public void Indexer_ForServiceAndResult_UpdatesCount(
        GattServices sut,
        IGattDeviceServiceWrapper service,
        IGattCharacteristicsResultWrapper result)
    {
        sut[service] = result;

        sut.Count
            .Should()
            .Be(1);
    }

    [AutoDataTestMethod]
    public void Indexer_ForExistingService_DisposesOldValueOnly(
        GattServices sut,
        IGattDeviceServiceWrapper service,
        IGattCharacteristicsResultWrapper first,
        IGattCharacteristicsResultWrapper second)
    {
        sut[service] = first;
        sut[service] = second;

        sut[service]
            .Should()
            .Be(second);

        first.Received(1)
            .Dispose();
        second.DidNotReceive()
            .Dispose();
        service.DidNotReceive()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Clear_ForInvoked_DisposesService1(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Clear();

        service1.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Clear_ForInvoked_DisposesService2(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Clear();

        service2.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Clear_ForInvoked_DisposesValues(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Clear();

        result1.Received()
            .Dispose();
        result2.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Clear_ForInvoked_SetsCountToZero(
        GattServices sut,
        IGattDeviceServiceWrapper service,
        IGattCharacteristicsResultWrapper result)
    {
        sut[service] = result;

        sut.Clear();

        sut.Count
            .Should()
            .Be(0);
    }

    [AutoDataTestMethod]
    public void Dispose_ForInvoked_DisposesService1(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Dispose();

        service1.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Dispose_ForInvoked_DisposesService2(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Dispose();

        service2.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void Dispose_ForInvoked_DisposesValues(
        GattServices sut,
        IGattDeviceServiceWrapper service1,
        IGattCharacteristicsResultWrapper result1,
        IGattDeviceServiceWrapper service2,
        IGattCharacteristicsResultWrapper result2)
    {
        sut[service1] = result1;
        sut[service2] = result2;

        sut.Dispose();

        result1.Received()
            .Dispose();
        result2.Received()
            .Dispose();
    }

    [AutoDataTestMethod]
    public void ReadOnlyDictionary_ForInvoked_ContainsService1(
        GattServices sut,
        IGattDeviceServiceWrapper service,
        IGattCharacteristicsResultWrapper result)
    {
        sut[service] = result;

        sut.ReadOnlyDictionary[service]
            .Should()
            .Be(result);
    }
}
