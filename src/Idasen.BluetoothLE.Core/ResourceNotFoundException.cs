namespace Idasen.BluetoothLE.Core ;

using Selkie.DefCon.One.Common ;

public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException (
        string resourceName ,
        [ GuardIgnore ] string? message )
        : base ( message )
    {
        Guard.ArgumentNotNull ( resourceName ,
                                nameof ( resourceName ) ) ;

        ResourceName = resourceName ;
    }

    public string ResourceName { get ; }
}
