using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[Intercept ( typeof ( LogAspect ) ) ]
public class DeskCommandsProvider
    : IDeskCommandsProvider
{
    private static readonly IReadOnlyDictionary < DeskCommands , byte [ ] > Commands =
        new Dictionary < DeskCommands , byte [ ] >
        {
            { DeskCommands.MoveUp , new byte [ ] { 0x47 , 0x00 } } ,
            { DeskCommands.MoveDown , new byte [ ] { 0x46 , 0x00 } } ,
            { DeskCommands.MoveStop , new byte [ ] { 0x48 , 0x00 } }
        } ;

    /// <inheritdoc />
    public bool TryGetValue ( DeskCommands command ,
                              out IEnumerable < byte > bytes )
    {
        if ( Commands.TryGetValue ( command , out var tempBytes ) )
        {
            bytes = tempBytes ;
            return true ;
        }

        bytes = Array.Empty < byte > ( ) ;
        return false ;
    }
}