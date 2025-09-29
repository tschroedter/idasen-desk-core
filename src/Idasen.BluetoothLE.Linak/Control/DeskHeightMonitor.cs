using Idasen.BluetoothLE.Linak.Interfaces;
using Serilog;

namespace Idasen.BluetoothLE.Linak.Control;

/// <inheritdoc />
public class DeskHeightMonitor
    : IDeskHeightMonitor
{
    public const int MinimumNumberOfItems = 5;
    private readonly ILogger _logger;

    private CircularBuffer<ulong> _history = new(MinimumNumberOfItems);

    public DeskHeightMonitor(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsHeightChanging()
    {
        if (_history.Size < MinimumNumberOfItems)
            return true;

        var needed = MinimumNumberOfItems;
        var skip = Math.Max(
            0,
            _history.Size - needed);
        var lastNValues = _history.Skip(skip)
            .ToArray();

        var differentValues = lastNValues.Distinct()
            .Count();

        _logger.Debug(
            "History: {History}; DifferentValues={DifferentValues}",
            string.Join(
                ",",
                lastNValues),
            differentValues);

        return differentValues > 1;
    }

    /// <inheritdoc />
    public void Reset() => _history = new CircularBuffer<ulong>(MinimumNumberOfItems);

    /// <inheritdoc />
    public void AddHeight(uint height) => _history.PushBack(height);
}
