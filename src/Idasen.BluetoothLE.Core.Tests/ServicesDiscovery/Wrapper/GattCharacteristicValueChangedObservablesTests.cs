using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using FluentAssertions;
using Idasen.BluetoothLE.Core.ServicesDiscovery.Wrappers;
using NSubstitute;
using Selkie.AutoMocking;
using Serilog;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery.Wrapper;

[AutoDataTestClass]
public class GattCharacteristicValueChangedObservablesTests : IDisposable
{
    private GattCharacteristicValueChangedDetails _details = null!;
    private ILogger _logger = null!;
    private IScheduler _scheduler = null!;
    private Subject<GattCharacteristicValueChangedDetails> _subject = null!;

    public void Dispose()
    {
        _subject?.Dispose();

        GC.SuppressFinalize(this);
    }

    [TestInitialize]
    public void Initialize()
    {
        _logger = Substitute.For<ILogger>();
        _subject = new Subject<GattCharacteristicValueChangedDetails>();
        _scheduler = Substitute.For<IScheduler>();
        _details = new GattCharacteristicValueChangedDetails(
            Guid.NewGuid(),
            [],
            DateTimeOffset.Now);
    }

    [AutoDataTestMethod]
    public void Create_ForInvoked_Instance(GattCharacteristicValueChangedObservables sut)
    {
        sut.Should()
            .NotBeNull();
    }

    [TestMethod]
    public void ValueChanged_ForSubscribe_Notifies()
    {
        var uuid = Guid.Empty;

        using var sut = new GattCharacteristicValueChangedObservables(
            _logger,
            _scheduler,
            _subject);

        using var disposable = sut.ValueChanged
            .Subscribe(x => { uuid = x.Uuid; });

        _subject.OnNext(_details);

        uuid.Should()
            .Be(_details.Uuid);
    }
}
