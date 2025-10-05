using JetBrains.Annotations ;

namespace Idasen.Aop ;

public static class StringExtensions
{
    [UsedImplicitly]
    public static string MaskMacAddress(this string macAddress)
    {
        if (macAddress.Length < 5 ||
            string.IsNullOrWhiteSpace(macAddress))
            return macAddress;

        return $"***-{macAddress[^5..]}";
    }
}
