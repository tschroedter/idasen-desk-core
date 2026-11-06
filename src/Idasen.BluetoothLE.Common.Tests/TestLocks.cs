namespace Idasen.BluetoothLE.Common.Tests
{
    internal static class TestLocks
    {
        // Shared lock used by test helpers to serialize operations that touch shared runtime state
        internal static readonly object CreateNonNullLock = new();
    }
}
