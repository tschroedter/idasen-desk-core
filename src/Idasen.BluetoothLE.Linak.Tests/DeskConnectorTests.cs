using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using FluentAssertions;
using Idasen.BluetoothLE.Common.Tests;
using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery;
using Idasen.BluetoothLE.Linak;
using Idasen.BluetoothLE.Linak.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Serilog;

namespace Idasen.BluetoothLE.Linak.Tests;

[TestClass]
public class DeskConnectorTests
{
    private ILogger _logger = null!;
    private IScheduler _scheduler = null!;
    private Subject<uint> _heightSubject = null!;
    private Subject<int> _speedSubject = null!;
    private Subject<bool> _refreshedSubject = null!;
    private Subject<HeightSpeedDetails> _heightSpeedSubject = null!;

    private IDevice _device = null!;
    private Subject<GattCommunicationStatus> _gattRefreshed = null!;

    private IDeskCharacteristics _deskCharacteristics = null!;
    private IGenericAccess _genericAccess = null!;
    private Subject<System.Collections.Generic.IEnumerable<byte>> _genericAccessDeviceNameChanged = null!;
    private IReferenceOutput _referenceOutput = null!;
    private IControl _control = null!;

    private IDeskHeightAndSpeedFactory _heightAndSpeedFactory = null!;
    private IDeskHeightAndSpeed _heightAndSpeed = null!;
    private Subject<uint> _heightChanged = null!;
    private Subject<int> _speedChanged = null!;
    private Subject<HeightSpeedDetails> _heightAndSpeedChanged = null!;

    private IDeskCommandExecutorFactory _commandExecutorFactory = null!;
    private IDeskCommandExecutor _executor = null!;

    private IDeskMoverFactory _moverFactory = null!;
    private IDeskMover _mover = null!;
    private Subject<uint> _moverFinished = null!;

    private IDeskLockerFactory _deskLockerFactory = null!;
    private IDeskLocker _deskLocker = null!;

    private IErrorManager _errorManager = null!;

    private DeskConnector CreateSut()
    {
        _logger = Substitute.For<ILogger>();
        // Use a real scheduler to ensure Rx SubscribeOn executes subscriptions.
        _scheduler = CurrentThreadScheduler.Instance;

        _heightSubject = new Subject<uint>();
        _speedSubject = new Subject<int>();
        _refreshedSubject = new Subject<bool>();
        _heightSpeedSubject = new Subject<HeightSpeedDetails>();

        _device = Substitute.For<IDevice>();
        _gattRefreshed = new Subject<GattCommunicationStatus>();
        _device.GattServicesRefreshed.Returns(_gattRefreshed);

        _deskCharacteristics = Substitute.For<IDeskCharacteristics>();
        _genericAccess = Substitute.For<IGenericAccess>();
        _genericAccessDeviceNameChanged = new Subject<System.Collections.Generic.IEnumerable<byte>>();
        _genericAccess.DeviceNameChanged.Returns(_genericAccessDeviceNameChanged);
        _referenceOutput = Substitute.For<IReferenceOutput>();
        _control = Substitute.For<IControl>();
        _deskCharacteristics.GenericAccess.Returns(_genericAccess);
        _deskCharacteristics.ReferenceOutput.Returns(_referenceOutput);
        _deskCharacteristics.Control.Returns(_control);
        _deskCharacteristics.Initialize(_device).Returns(_deskCharacteristics);

        _heightAndSpeedFactory = Substitute.For<IDeskHeightAndSpeedFactory>();
        _heightAndSpeed = Substitute.For<IDeskHeightAndSpeed>();
        _heightChanged = new Subject<uint>();
        _speedChanged = new Subject<int>();
        _heightAndSpeedChanged = new Subject<HeightSpeedDetails>();
        _heightAndSpeed.HeightChanged.Returns(_heightChanged);
        _heightAndSpeed.SpeedChanged.Returns(_speedChanged);
        _heightAndSpeed.HeightAndSpeedChanged.Returns(_heightAndSpeedChanged);
        _heightAndSpeed.Initialize().Returns(_heightAndSpeed);
        _heightAndSpeedFactory.Create(_referenceOutput).Returns(_heightAndSpeed);

        _commandExecutorFactory = Substitute.For<IDeskCommandExecutorFactory>();
        _executor = Substitute.For<IDeskCommandExecutor>();
        _commandExecutorFactory.Create(_control).Returns(_executor);

        _moverFactory = Substitute.For<IDeskMoverFactory>();
        _mover = Substitute.For<IDeskMover>();
        _moverFinished = new Subject<uint>();
        _mover.Finished.Returns(_moverFinished);
        _moverFactory.Create(_executor, _heightAndSpeed).Returns(_mover);

        _deskLockerFactory = Substitute.For<IDeskLockerFactory>();
        _deskLocker = Substitute.For<IDeskLocker>();
        _deskLockerFactory.Create(_mover, _executor, _heightAndSpeed).Returns(_deskLocker);

        _errorManager = Substitute.For<IErrorManager>();

        return new DeskConnector(
            _logger,
            _scheduler,
            () => new Subject<System.Collections.Generic.IEnumerable<byte>>(),
            _heightSubject,
            _speedSubject,
            _refreshedSubject,
            _heightSpeedSubject,
            _device,
            _deskCharacteristics,
            _heightAndSpeedFactory,
            _commandExecutorFactory,
            _moverFactory,
            _deskLockerFactory,
            _errorManager);
    }

    private static async Task InvokeOnGattServicesRefreshedAsync(DeskConnector sut, GattCommunicationStatus status)
    {
        var method = typeof(DeskConnector).GetMethod("OnGattServicesRefreshed", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, new object[] { status })!;
        await task.ConfigureAwait(false);
    }

    [TestMethod]
    public async Task MoveUp_BeforeRefresh_ReturnsFalseAndLogs()
    {
        var sut = CreateSut();

        var result = await sut.MoveUp();

        result.Should().BeFalse();
        _logger.Received().Error(Arg.Is<string>(s => s.Contains("refreshed", StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public async Task MoveUp_AfterRefresh_CallsMoverUp()
    {
        var sut = CreateSut();
        _mover.Up().Returns(Task.FromResult(true));

        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        var result = await sut.MoveUp();

        result.Should().BeTrue();
        await _mover.Received().Up();
    }

    [TestMethod]
    public async Task MoveDown_Stop_AfterRefresh_CallMover()
    {
        var sut = CreateSut();
        _mover.Down().Returns(Task.FromResult(true));
        _mover.Stop().Returns(Task.FromResult(true));

        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        (await sut.MoveDown()).Should().BeTrue();
        await _mover.Received().Down();

        (await sut.MoveStop()).Should().BeTrue();
        await _mover.Received().Stop();
    }

    [TestMethod]
    public async Task MoveLockUnlock_BeforeAndAfterRefresh()
    {
        var sut = CreateSut();

        (await sut.MoveLock()).Should().BeFalse();
        (await sut.MoveUnlock()).Should().BeFalse();

        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        (await sut.MoveLock()).Should().BeTrue();
        _deskLocker.Received().Lock();

        (await sut.MoveUnlock()).Should().BeTrue();
        _deskLocker.Received().Unlock();
    }

    [TestMethod]
    public async Task MoveTo_Zero_ThrowsAfterRefresh()
    {
        var sut = CreateSut();
        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        var act = () => Task.Run(() => sut.MoveTo(0));
        var assertions = await act.Should().ThrowAsync<ArgumentException>();
        assertions.WithParameter("targetHeight");
    }

    [TestMethod]
    public async Task Streams_Forwarded_From_HeightSpeed_And_Mover_Finished()
    {
        var sut = CreateSut();

        uint? receivedHeight = null;
        int? receivedSpeed = null;
        HeightSpeedDetails? receivedDetails = null;
        uint? receivedFinished = null;

        using var h = sut.HeightChanged.Subscribe(v => receivedHeight = v);
        using var s = sut.SpeedChanged.Subscribe(v => receivedSpeed = v);
        using var hs = sut.HeightAndSpeedChanged.Subscribe(v => receivedDetails = v);
        using var f = sut.FinishedChanged.Subscribe(v => receivedFinished = v);

        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        _heightChanged.OnNext(100u);
        _speedChanged.OnNext(5);
        var details = new HeightSpeedDetails(DateTimeOffset.Now, 100u, 5);
        _heightAndSpeedChanged.OnNext(details);
        _moverFinished.OnNext(123u);

        receivedHeight.Should().Be(100u);
        receivedSpeed.Should().Be(5);
        receivedDetails.Should().NotBeNull();
        receivedDetails!.Height.Should().Be(100u);
        receivedDetails.Speed.Should().Be(5);
        receivedFinished.Should().Be(123u);
    }

    [TestMethod]
    public async Task DeviceNameChanged_Forwarded()
    {
        var sut = CreateSut();

        System.Collections.Generic.IEnumerable<byte>? received = null;
        using var sub = sut.DeviceNameChanged.Subscribe(v => received = v);

        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        var name = new byte[] { 65, 66, 67 };
        _genericAccessDeviceNameChanged.OnNext(name);

        received.Should().NotBeNull();
        received.Should().BeEquivalentTo(name);
    }

    [TestMethod]
    public async Task Dispose_Disposes_And_Completes_Subjects()
    {
        var sut = CreateSut();
        await InvokeOnGattServicesRefreshedAsync(sut, GattCommunicationStatus.Success);

        var finishedCompleted = false;
        var deviceNameCompleted = false;

        using var f = sut.FinishedChanged.Subscribe(_ => { }, () => finishedCompleted = true);
        using var d = sut.DeviceNameChanged.Subscribe(_ => { }, () => deviceNameCompleted = true);

        sut.Dispose();

        _deskLocker.Received().Dispose();
        _mover.Received().Dispose();
        _heightAndSpeed.Received().Dispose();
        _device.Received().Dispose();

        finishedCompleted.Should().BeTrue();
        deviceNameCompleted.Should().BeTrue();
    }
}
