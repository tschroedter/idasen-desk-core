#nullable disable

namespace Idasen.BluetoothLE.Core.Tests
{
    public static class GuardTestData
    {
        public static IEnumerable < object [ ] > InstanceAndInteger ( )
        {
            yield return
            [
                Instance
            ] ;
            yield return
            [
                Integer
            ] ;
        }

        public static IEnumerable < object [ ] > NullEmptyOrWhitespace ( )
        {
            yield return
            [
                null ,
                typeof ( ArgumentNullException )
            ] ;
            yield return
            [
                Empty ,
                typeof ( ArgumentException )
            ] ;
            yield return
            [
                Whitespace ,
                typeof ( ArgumentException )
            ] ;
        }

        public static IEnumerable < object [ ] > NullOrEmpty ( )
        {
            yield return
            [
                null ,
                typeof ( ArgumentNullException )
            ] ;
            yield return
            [
                Empty ,
                typeof ( ArgumentException )
            ] ;
        }

        public const string Empty      = "" ;
        public const string Whitespace = " " ;

        private const int Integer = 1 ;

        private static readonly object Instance = new( ) ;
    }
}