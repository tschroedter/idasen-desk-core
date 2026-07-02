using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Linak.Interfaces ;
using NSubstitute ;
using Selkie.AutoMocking ;

namespace Idasen.BluetoothLE.Linak.Tests ;

[ AutoDataTestClass ]
public class DeskProviderTests
{
    [ AutoDataTestMethod ]
    public void Initialize_ForDeviceNameIsNull_Throws ( DeskProvider sut ,
                                                        ulong        deviceAddress ,
                                                        uint         deviceTimeout )
    {
        Action action = ( ) => sut.Initialize ( null! ,
                                                deviceAddress ,
                                                deviceTimeout ) ;

        action.Should ( )
              .Throw < ArgumentException > ( )
              .WithParameter ( "deviceName" ) ;
    }

    [ AutoDataTestMethod ]
    public void Initialize_ForInvoked_CallsDetectorInitialize ( DeskProvider             sut ,
                                                                [ Freeze ] IDeskDetector detector ,
                                                                string                   deviceName ,
                                                                ulong                    deviceAddress ,
                                                                uint                     deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout ) ;

        detector.Received ( )
                .Initialize ( deviceName ,
                              deviceAddress ,
                              deviceTimeout ) ;
    }

    [ AutoDataTestMethod ]
    public void Initialize_ForInvoked_SubscribesToDeskDetected ( DeskProvider                     sut ,
                                                                 [ Freeze ] IObservable < IDesk > deskDetected ,
                                                                 string                           deviceName ,
                                                                 ulong                            deviceAddress ,
                                                                 uint                             deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout ) ;

        deskDetected.ReceivedWithAnyArgs ( )
                    .Subscribe ( ) ;
    }

    [ AutoDataTestMethod ]
    public void StartDetecting_ForInvoked_CallsDeskDetectorStart ( DeskProvider             sut ,
                                                                   [ Freeze ] IDeskDetector detector ,
                                                                   string                   deviceName ,
                                                                   ulong                    deviceAddress ,
                                                                   uint                     deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .StartDetecting ( ) ;

        detector.Received ( )
                .StartListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void StopDetecting_ForInvoked_CallsDeskDetectorStart ( DeskProvider             sut ,
                                                                  [ Freeze ] IDeskDetector detector ,
                                                                  string                   deviceName ,
                                                                  ulong                    deviceAddress ,
                                                                  uint                     deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .StopDetecting ( ) ;

        detector.Received ( )
                .StopListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void Dispose_ForInvoked_DisposesDeskDetected ( DeskProvider           sut ,
                                                          [ Freeze ] IDisposable deskDetected ,
                                                          string                 deviceName ,
                                                          ulong                  deviceAddress ,
                                                          uint                   deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .Dispose ( ) ;

        deskDetected.Received ( )
                    .Dispose ( ) ;
    }

    [ AutoDataTestMethod ]
    public void StopDetecting_ForInvoked_DisposesDeskDetector ( DeskProvider             sut ,
                                                                [ Freeze ] IDeskDetector detector ,
                                                                string                   deviceName ,
                                                                ulong                    deviceAddress ,
                                                                uint                     deviceTimeout )
    {
        sut.Initialize ( deviceName ,
                         deviceAddress ,
                         deviceTimeout )
           .Dispose ( ) ;

        detector.Received ( )
                .Dispose ( ) ;
    }

    [ AutoDataTestMethod ]
    public void DeskDetected_ForInvoked_CallsDeskDetector ( DeskProvider                     sut ,
                                                            [ Freeze ] IObservable < IDesk > deskDetected )
    {
        sut.DeskDetected
           .Subscribe ( ) ;

        deskDetected.ReceivedWithAnyArgs ( )
                    .Subscribe ( ) ;
    }

    [ AutoDataTestMethod ]
    public async Task TryGetDesk_ForInvoked_CallsDeskDetectorStart ( DeskProvider             sut ,
                                                                     [ Freeze ] IDeskDetector detector ,
                                                                     CancellationTokenSource  source ,
                                                                     string                   deviceName ,
                                                                     ulong                    deviceAddress ,
                                                                     uint                     deviceTimeout )
    {
        await sut.Initialize ( deviceName ,
                               deviceAddress ,
                               deviceTimeout )
                 .TryGetDesk ( source.Token ) ;

        detector.Received ( )
                .StartListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public async Task TryGetDesk_ForCancelled_ReturnsFalse ( DeskProvider             sut ,
                                                             [ Freeze ] IDeskDetector detector ,
                                                             CancellationTokenSource  source ,
                                                             string                   deviceName ,
                                                             ulong                    deviceAddress ,
                                                             uint                     deviceTimeout )
    {
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

    [ AutoDataTestMethod ]
    public async Task TryGetDesk_ForCancelled_ReturnsNullForDesk ( DeskProvider             sut ,
                                                                   [ Freeze ] IDeskDetector detector ,
                                                                   CancellationTokenSource  source ,
                                                                   string                   deviceName ,
                                                                   ulong                    deviceAddress ,
                                                                   uint                     deviceTimeout )
    {
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

    [ AutoDataTestMethod ]
    public void OnDeskDetected_ForInvoked_CallsDeskDetectorStop ( DeskProvider             sut ,
                                                                  [ Freeze ] IDeskDetector detector ,
                                                                  IDesk                    desk )
    {
        sut.OnDeskDetected ( desk ) ;

        detector.Received ( )
                .StopListening ( ) ;
    }

    [ AutoDataTestMethod ]
    public void OnDeskDetected_ForInvoked_SetsDesk ( DeskProvider sut ,
                                                     IDesk        desk )
    {
        sut.OnDeskDetected ( desk ) ;

        sut.Desk
           .Should ( )
           .Be ( desk ) ;
    }

    [ AutoDataTestMethod ]
    public async Task OnDeskDetected_ForInvoked_CallsDeskDetectedEventSet ( DeskProvider            sut ,
                                                                            IDesk                   desk ,
                                                                            CancellationTokenSource source )
    {
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

    [ AutoDataTestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_StartsWithInitialWait ( DeskProvider            sut ,
                                                                            CancellationTokenSource source )
    {
        // Safety timeout so the test doesn't hang
        source.CancelAfter ( TimeSpan.FromSeconds ( 2 ) ) ;

        var startTime = DateTime.UtcNow ;
        sut.DoTryGetDesk ( source.Token ) ;
        var elapsed = DateTime.UtcNow - startTime ;

        // Should wait at least the initial 1 second (with small tolerance)
        elapsed.Should ( )
               .BeGreaterThan ( TimeSpan.FromMilliseconds ( 900 ) ) ;
    }

    [ AutoDataTestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_DoublesWaitTimeOnTimeout ( DeskProvider            sut ,
                                                                               CancellationTokenSource source )
    {
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

    [ AutoDataTestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_CapsAtMaximumWait ( DeskProvider            sut ,
                                                                        CancellationTokenSource source )
    {
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

    [ AutoDataTestMethod ]
    public async Task DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyOnDeskDetection ( DeskProvider            sut ,
                                                                                            IDesk                   desk ,
                                                                                            CancellationTokenSource source )
    {
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

    [ AutoDataTestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyWhenCancelled ( DeskProvider            sut ,
                                                                                    CancellationTokenSource source )
    {
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

    [ AutoDataTestMethod ]
    public void DoTryGetDesk_WithExponentialBackoff_ExitsImmediatelyIfDeskAlreadySet ( DeskProvider            sut ,
                                                                                       IDesk                   desk ,
                                                                                       CancellationTokenSource source )
    {
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
}
