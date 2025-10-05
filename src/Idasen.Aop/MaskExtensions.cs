namespace Idasen.Aop ;

public static class MaskExtensions
{
    public static string MaskMacAddress ( this string macAddress )
    {
        if ( macAddress.Length < 5 ||
             string.IsNullOrWhiteSpace ( macAddress ) )
            return macAddress ;

        return $"***-{macAddress [ ^5.. ]}" ;
    }

    public static string MaskAddress ( this ulong address )
    {
        var text = address.ToString ( ).PadLeft ( 20 ,
                                                  '0' ) ;

        if ( text.Length < 5 ||
             string.IsNullOrWhiteSpace ( text ) )
            return text ;

        return $"***{text [ ^5.. ]}" ;
    }
}
