using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics;

namespace Idasen.BluetoothLE.Characteristics.Characteristics.Unknowns;

public class GenericAttributeService
    : UnknownBase,
        IGenericAttributeService
{
    public IEnumerable<byte> RawServiceChanged { get; } = RawArrayEmpty;
}
