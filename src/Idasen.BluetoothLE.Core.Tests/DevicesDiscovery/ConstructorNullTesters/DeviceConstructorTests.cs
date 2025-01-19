using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.DevicesDiscovery ;
using Microsoft.VisualStudio.TestTools.UnitTesting ;

namespace Idasen.BluetoothLE.Core.Tests.DevicesDiscovery.ConstructorNullTesters ;

[ TestClass ]
public class DeviceConstructorTests
    : BaseConstructorNullTester < Device >
{
    public override int NumberOfConstructorsPassed { get ; } = 2 ;
}