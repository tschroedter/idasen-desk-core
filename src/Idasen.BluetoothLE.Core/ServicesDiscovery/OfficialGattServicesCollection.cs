using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Autofac.Extras.DynamicProxy;
using CsvHelper;
using Idasen.Aop.Aspects;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery.Wrappers;

namespace Idasen.BluetoothLE.Core.ServicesDiscovery;

/// <inheritdoc />
[Intercept(typeof(LogAspect))]
public class OfficialGattServicesCollection
    : IOfficialGattServicesCollection
{
    private const string FileName = "OfficialGattServicesCollection.txt";

    private readonly Dictionary<ushort, OfficialGattService> _dictionary = new();

    public OfficialGattServicesCollection()
    {
        ResourceName = GetType().Namespace + "." + FileName;

        Populate(ReadCsvFile(ResourceName));
    }

    /// <summary>
    ///     Gets the manifest resource name of the embedded CSV file.
    /// </summary>
    public string ResourceName { get; }

    /// <inheritdoc />
    public IEnumerator<OfficialGattService> GetEnumerator()
    {
        return _dictionary.Values.GetEnumerator();
    }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <inheritdoc />
    public int Count => _dictionary.Count;

    /// <inheritdoc />
    public bool TryFindByUuid(Guid uuid,
                                out OfficialGattService? gattService)
    {
        gattService = null;

        var n = uuid.ToString("N");

        if (n.Length < 8)
            return false;

        var number = n.Substring(4,
                                   4);

        // Avoid exception-based control flow; validate parse
        if (!ushort.TryParse(number,
                                 NumberStyles.HexNumber,
                                 CultureInfo.InvariantCulture,
                                 out var assignedNumber))
            return false;

        return _dictionary.TryGetValue(assignedNumber,
                                         out gattService);
    }

    private void Populate(IEnumerable<OfficialGattService> records)
    {
        foreach (var record in records) _dictionary[record.AssignedNumber] = record;
    }

    private static OfficialGattService[] ReadCsvFile(string resourceName)
    {
        var stream = typeof(OfficialGattServicesCollection).Assembly
                                                              .GetManifestResourceStream(resourceName);

        if (stream == null)
            throw new ResourceNotFoundException(resourceName,
                                                  $"Can't find resource {resourceName}");

        using var reader = new StreamReader(stream);

        var options = new CsvReaderOptions {
            Separator = ',',
            HasHeader = true
        };

        using var csv = CsvReader.Create(reader,
                                         options);

        // Class maps are replaced by [CsvRecordMap] attributes in v34, 
        // so explicit registration via RegisterClassMap is no longer needed.

        var readCsvFile = csv.GetRecords<OfficialGattService>()
                             .ToArray();

        return readCsvFile;
    }
}
