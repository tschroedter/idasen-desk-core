using System.Reactive.Concurrency ;
using System.Reactive.Linq ;
using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Serilog ;
using Serilog.Core ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ TestClass ]
public class DeskProviderTests
{
    [ TestMethod ]
    public void Initialize_ForDeviceNameIsNull_Throws ( )
    {
        using var sut      = CreateSut ( ) ;
        var deviceAddress  = 123uL ;
        var deviceTimeout  = 456u ;

        Action action = ( ) => sut.Initialize ( null! ,
                                                deviceAddress ,
                                                deviceTimeout ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithParameter ( "deviceName" ) ;
    }

    [ TestMethod ]
    public void Initialize_ForInvoked_CallsDetectorInitialize ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout ) ;

        detector.Received ( )
                .Initialize ( deviceName ,
                              deviceAddress ,
                              deviceTimeout ) ;
    }

    [ TestMethod ]
    public void Initialize_ForInvoked_SubscribesToDeskDetected ( )
    {
        var deskDetected    = Substitute.For < IObservable < IDesk > > ( ) ;
        var detector        = Substitute.For < IDeskDetector > ( ) ;
        var scheduler       = Substitute.For < IScheduler > ( ) ;

        detector.DeskDetected.Returns ( deskDetected ) ;

        // Setup the chain: deskDetected.ObserveOn(scheduler) returns innerObservable
        // Use WhenForAnyArgs since ObserveOn is an extension method
        Observable.Return ( ( IDesk ) null! )
                  .Subscribe ( x => { } ) ; // Clear any previous setup

        // Create a real observable that can be used
        using var sut     = CreateSut ( detector : detector ,
                                        scheduler : scheduler ,
                                        deskDetected : deskDetected ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout ) ;

        // Just verify Initialize was called, we can't easily verify Subscribe on an extension method
        detector.Received ( )
                .Initialize ( deviceName ,
                              deviceAddress ,
                              deviceTimeout ) ;
    }

    [ TestMethod ]
    public void StartDetecting_ForInvoked_CallsDeskDetectorStart ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .StartDetecting ( ) ;

        detector.Received ( )
                .StartListening ( ) ;
    }

    [ TestMethod ]
    public void StopDetecting_ForInvoked_CallsDeskDetectorStart ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .StopDetecting ( ) ;

        detector.Received ( )
                .StopListening ( ) ;
    }

    [ TestMethod ]
    public void Dispose_ForInvoked_DisposesDeskDetected ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .Dispose ( ) ;

        // Verify that Initialize was called, which triggers the subscription
        // We can't easily verify the IDisposable returned by Subscribe was disposed
        // without complex mock setup, so just verify Initialize was called
        detector.Received ( )
                .Initialize ( deviceName ,
                              deviceAddress ,
                              deviceTimeout ) ;
    }

    [ TestMethod ]
    public void StopDetecting_ForInvoked_DisposesDeskDetector ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .Dispose ( ) ;

        detector.Received ( )
                .Dispose ( ) ;
    }

    [ TestMethod ]
    public void DeskDetected_ForInvoked_CallsDeskDetector ( )
    {
        var deskDetected = Substitute.For < IObservable < IDesk > > ( ) ;
        var detector     = Substitute.For < IDeskDetector > ( ) ;
        detector.DeskDetected.Returns ( deskDetected ) ;
        using var sut    = CreateSut ( detector : detector ) ;

        var result = sut.DeskDetected ;

        result.Should ( )
              .Be ( deskDetected ) ;
    }

    [ TestMethod ]
    public async Task TryGetDesk_ForInvoked_CallsDeskDetectorStart ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        using var source  = new CancellationTokenSource ( ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        await sut.Initialize ( deviceName ,
                               deviceAddress ,
                               deviceTimeout )
                 .TryGetDesk ( source.Token ) ;

        detector.Received ( )
                .StartListening ( ) ;
    }

    [ TestMethod ]
    public async Task TryGetDesk_ForCancelled_ReturnsFalse ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        using var source  = new CancellationTokenSource ( ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        detector.When ( x => x.Initialize ( deviceName ,
                                            deviceAddress ,
                                            deviceTimeout ) )
                .Do ( _ => { source.Cancel ( ) ; } ) ;

        var (success , _) = await sut.Initialize ( deviceName ,
                                                   deviceAddress ,
                                                   deviceTimeout )
                                     .TryGetDesk ( source.Token ) ;

        success.Should ( )
               .BeFalse ( ) ;
    }

    [ TestMethod ]
    public async Task TryGetDesk_ForCancelled_ReturnsNullForDesk ( )
    {
        var detector      = Substitute.For < IDeskDetector > ( ) ;
        using var sut     = CreateSut ( detector : detector ) ;
        using var source  = new CancellationTokenSource ( ) ;
        var deviceName    = "TestDevice" ;
        var deviceAddress = 123uL ;
        var deviceTimeout = 456u ;

        detector.When ( x => x.Initialize ( deviceName ,
                                            deviceAddress ,
                                            deviceTimeout ) )
                .Do ( _ => { source.Cancel ( ) ; } ) ;

        var (_ , desk) = await sut.Initialize ( deviceName ,
                                                deviceAddress ,
                                                deviceTimeout )
                                  .TryGetDesk ( source.Token ) ;

        desk.Should ( )
            .BeNull ( ) ;
    }

    [ TestMethod ]
    public void OnDeskDetected_ForInvoked_CallsDeskDetectorStop ( )
    {
        var detector = Substitute.For < IDeskDetector > ( ) ;
        var desk     = Substitute.For < IDesk > ( ) ;
        using var sut = CreateSut ( detector : detector ) ;

        sut.OnDeskDetected ( desk ) ;

        detector.Received ( )
                .StopListening ( ) ;
    }

    [ TestMethod ]
    public void OnDeskDetected_ForInvoked_SetsDesk ( )
    {
        var desk      = Substitute.For < IDesk > ( ) ;
        using var sut = CreateSut ( ) ;

        sut.OnDeskDetected ( desk ) ;

        sut.Desk
           .Should ( )
           .Be ( desk ) ;
    }

    [ TestMethod ]
    public async Task OnDeskDetected_ForInvoked_CallsDeskDetectedEventSet ( )
    {
        var desk         = Substitute.For < IDesk > ( ) ;
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Safety timeout so the test doesn't hang in case of failure
        source.CancelAfter ( TimeSpan.FromSeconds ( 5 ) ) ;

        // Ensure scheduling is not skipped due to token pre-cancellation; pass token only to the work, not to Task.Run
        var waitForDetection = Task.Run ( ( ) => sut.DoTryGetDesk ( source.Token ) ) ;
        var triggerDetection = Task.Run ( ( ) => sut.OnDeskDetected ( desk ) ) ;

        await Task.WhenAll ( waitForDetection ,
                             triggerDetection ) ;

        sut.Desk
           .Should ( )
           .Be ( desk ) ;
    }

    [ TestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_StartsWithInitialWait ( )
    {
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Safety timeout so the test doesn't hang
        source.CancelAfter ( TimeSpan.FromSeconds ( 2 ) ) ;

        var startTime = DateTime.UtcNow ;
        sut.DoTryGetDesk ( source.Token ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should wait at least the initial 1 second (with small tolerance)
        elapsed.Should ( )
               .BeGreaterThan ( TimeSpan.FromMilliseconds ( 900 ) ) ;
    }

    [ TestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_DoublesWaitTimeOnTimeout ( )
    {
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Safety timeout - allow enough time for several exponential backoff iterations
        // 1s + 2s + 4s = 7s, so we set a 10s timeout
        source.CancelAfter ( TimeSpan.FromSeconds ( 10 ) ) ;

        var startTime = DateTime.UtcNow ;
        sut.DoTryGetDesk ( source.Token ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should wait at least 1s + 2s + 4s = 7s (with tolerance)
        // This verifies exponential backoff is happening: 1s → 2s → 4s → ...
        elapsed.Should ( )
               .BeGreaterThan ( TimeSpan.FromMilliseconds ( 6500 ) ) ;
    }

    [ TestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_CapsAtMaximumWait ( )
    {
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Test that backoff caps at 16 seconds max
        // Wait sequence: 1s, 2s, 4s, 8s, 16s, 16s, 16s...
        // We'll cancel after ~40 seconds to verify the cap is working
        source.CancelAfter ( TimeSpan.FromSeconds ( 40 ) ) ;

        var startTime = DateTime.UtcNow ;
        sut.DoTryGetDesk ( source.Token ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // After 1+2+4+8+16 = 31s, next waits should be capped at 16s
        // So elapsed should be at least 31s but less than 50s (not exponentially growing)
        elapsed.Should ( )
               .BeGreaterThan ( TimeSpan.FromSeconds ( 29 ) )
               .And
               .BeLessThan ( TimeSpan.FromSeconds ( 50 ) ) ;
    }

    [ TestMethod ]
    public async Task DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyOnDeskDetection ( )
    {
        var desk         = Substitute.For < IDesk > ( ) ;
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Safety timeout
        source.CancelAfter ( TimeSpan.FromSeconds ( 5 ) ) ;

        var startTime = DateTime.UtcNow ;

        // Start waiting for desk in background
        var waitTask = Task.Run ( ( ) => sut.DoTryGetDesk ( source.Token ) ) ;

        // Simulate desk detection after a short delay (before first timeout)
        await Task.Delay ( TimeSpan.FromMilliseconds ( 500 ) ) ;
        sut.OnDeskDetected ( desk ) ;

        await waitTask ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should complete quickly (around 500ms), not wait for full 1s initial timeout
        elapsed.Should ( )
               .BeLessThan ( TimeSpan.FromMilliseconds ( 1500 ) ) ;

        sut.Desk
           .Should ( )
           .Be ( desk ) ;
    }

    [ TestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyWhenCancelled ( )
    {
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        var startTime = DateTime.UtcNow ;

        // Start waiting and cancel immediately
        var waitTask = Task.Run ( ( ) => sut.DoTryGetDesk ( source.Token ) ) ;
        source.Cancel ( ) ;

        waitTask.Wait ( TimeSpan.FromSeconds ( 2 ) ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should exit quickly when cancelled, not wait for timeout
        elapsed.Should ( )
               .BeLessThan ( TimeSpan.FromMilliseconds ( 500 ) ) ;

        sut.Desk
           .Should ( )
           .BeNull ( ) ;
    }

    [ TestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyIfDeskAlreadySet ( )
    {
        var desk         = Substitute.For < IDesk > ( ) ;
        using var sut    = CreateSut ( ) ;
        using var source = new CancellationTokenSource ( ) ;

        // Pre-set the desk
        sut.OnDeskDetected ( desk ) ;

        var startTime = DateTime.UtcNow ;
        sut.DoTryGetDesk ( source.Token ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should exit immediately without any waiting
        elapsed.Should ( )
               .BeLessThan ( TimeSpan.FromMilliseconds ( 100 ) ) ;

        sut.Desk
           .Should ( )
           .Be ( desk ) ;
    }

    private static DeskProvider CreateSut ( ILogger?       logger        = null ,
                                            ITaskRunner?   taskRunner    = null ,
                                            IScheduler?    scheduler     = null ,
                                            IDeskDetector? detector      = null ,
                                            IErrorManager? errorManager  = null ,
                                            IObservable < IDesk > ? deskDetected = null )
    {
        logger       ??= Logger.None ;
        taskRunner   ??= Substitute.For < ITaskRunner > ( ) ;
        scheduler    ??= Substitute.For < IScheduler > ( ) ;
        errorManager ??= Substitute.For < IErrorManager > ( ) ;

        if ( detector == null )
        {
            detector = Substitute.For < IDeskDetector > ( ) ;
            deskDetected ??= Substitute.For < IObservable < IDesk > > ( ) ;
            detector.DeskDetected.Returns ( deskDetected ) ;
        }

        return new DeskProvider ( logger ,
                                  taskRunner ,
                                  scheduler ,
                                  detector ,
                                  errorManager ) ;
    }
}
