using CsvHelper ;
using CsvHelper.Configuration ;
using FluentAssertions ;
using Idasen.BluetoothLE.Common.Tests ;
using Idasen.BluetoothLE.Core.ServicesDiscovery ;
using NSubstitute ;

namespace Idasen.BluetoothLE.Core.Tests.ServicesDiscovery
{
    [ TestClass ]
    public class OfficialGattServiceConverterTests
    {
        [ TestInitialize ]
        public void Initialize ( )
        {
            _text      = "1800" ;
            _value     = 6144u ;
            _readerRow = Substitute.For < IReaderRow > ( ) ;
            _writerRow = Substitute.For < IWriterRow > ( ) ;

            var memberInfo = typeof ( TestClass ).GetMember ( "One" )
                                                 .First ( ) ;

            _memberMapData = new MemberMapData ( memberInfo ) ;
        }

        [ TestMethod ]
        public void ConvertFromString_ForTextNull_Zero ( )
        {
            CreateSut ( ).ConvertFromString ( null! ,
                                              _readerRow ,
                                              _memberMapData )
                         .Should ( )
                         .Be ( 0 ) ;
        }

        [ TestMethod ]
        public void ConvertFromString_ForReaderRowNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).ConvertFromString ( _text ,
                                                               null! ,
                                                               _memberMapData ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "readerRow" ) ;
        }

        [ TestMethod ]
        public void ConvertFromString_ForMemberMapDataNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).ConvertFromString ( _text ,
                                                               _readerRow ,
                                                               null! ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "memberMapData" ) ;
        }

        [ TestMethod ]
        public void ConvertFromString_ForTextIsNumber_Number ( )
        {
            CreateSut ( ).ConvertFromString ( _text ,
                                              _readerRow ,
                                              _memberMapData )
                         .Should ( )
                         .Be ( 6144u ) ;
        }

        [ TestMethod ]
        public void ConvertFromString_ForTextIsNotANumber_MaxValue ( )
        {
            CreateSut ( ).ConvertFromString ( NotANumber ,
                                              _readerRow ,
                                              _memberMapData )
                         .Should ( )
                         .Be ( ushort.MaxValue ) ;
        }

        [ TestMethod ]
        public void ConvertToString_ForValueNull_NullText ( )
        {
            CreateSut ( ).ConvertToString ( null! ,
                                            _writerRow ,
                                            _memberMapData )
                         .Should ( )
                         .Be ( "null" ) ;
        }

        [ TestMethod ]
        public void ConvertToString_ForWriterRowNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).ConvertToString ( _value ,
                                                             null! ,
                                                             _memberMapData ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "writerRow" ) ;
        }

        [ TestMethod ]
        public void ConvertToString_ForMemberMapDataNull_Throws ( )
        {
            var action = ( ) =>
                         {
                             CreateSut ( ).ConvertToString ( _value ,
                                                             _writerRow ,
                                                             null! ) ;
                         } ;

            action.Should ( )
                  .Throw < ArgumentNullException > ( )
                  .WithParameter ( "memberMapData" ) ;
        }

        [ TestMethod ]
        public void ConvertToString_ForValueIsNumber_String ( )
        {
            CreateSut ( ).ConvertToString ( _value ,
                                            _writerRow ,
                                            _memberMapData )
                         .Should ( )
                         .Be ( "6144" ) ;
        }

        [ TestMethod ]
        public void ConvertToString_ForValueIsNotANumber_String ( )
        {
            CreateSut ( ).ConvertToString ( NotANumber ,
                                            _writerRow ,
                                            _memberMapData )
                         .Should ( )
                         .Be ( NotANumber ) ;
        }

        private OfficialGattServiceConverter CreateSut ( )
        {
            return new OfficialGattServiceConverter ( ) ;
        }

        private const string NotANumber = "not a number" ;

        private MemberMapData _memberMapData = null! ;
        private IReaderRow    _readerRow     = null! ;
        private string        _text          = null! ;
        private uint          _value ;
        private IWriterRow    _writerRow = null! ;

        public class TestClass
        {
            public int One = 0 ;
        }
    }
}