using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control
{
    [ Intercept ( typeof ( LogAspect ) ) ]
    public class DeskCommandsProvider
        : IDeskCommandsProvider
    {
        public DeskCommandsProvider ( )
        {
            _dictionary.Add ( DeskCommands.MoveUp ,
                              new byte [ ] { 0x47 , 0x00 } ) ;
            _dictionary.Add ( DeskCommands.MoveDown ,
                              new byte [ ] { 0x46 , 0x00 } ) ;
            _dictionary.Add ( DeskCommands.MoveStop ,
                              new byte [ ] { 0x48 , 0x00 } ) ;
        }

        public bool TryGetValue ( DeskCommands             command ,
                                  out IEnumerable < byte > bytes )
        {
            var tryGetValue = _dictionary.TryGetValue ( command ,
                                                        out var tempBytes ) ;

            bytes = tempBytes ?? Array.Empty < byte > ( ) ;

            return tryGetValue ;
        }

        private readonly Dictionary < DeskCommands , IEnumerable < byte > > _dictionary = new( ) ;
    }
}