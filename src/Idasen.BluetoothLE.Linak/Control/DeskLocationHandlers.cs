using System.Diagnostics.CodeAnalysis ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

[ ExcludeFromCodeCoverage ]
public class DeskLocationHandlers ( IDeskHeightAndSpeed                   heightAndSpeed ,
                                    IInitialHeightAndSpeedProviderFactory providerFactory )
    : IDeskLocationHandlers
{
    public IDeskHeightAndSpeed                   HeightAndSpeed  { get ; } = heightAndSpeed ;
    public IInitialHeightAndSpeedProviderFactory ProviderFactory { get ; } = providerFactory ;
}