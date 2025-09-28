using Autofac.Extras.DynamicProxy ;
using Idasen.Aop.Aspects ;
using Idasen.BluetoothLE.Linak.Interfaces ;

namespace Idasen.BluetoothLE.Linak.Control ;

/// <inheritdoc />
[ Intercept ( typeof ( LogAspect ) ) ]
public class DeskCommandsProvider
    : IDeskCommandsProvider
{
    private static readonly IReadOnlyDictionary < DeskCommands , byte [ ] > Commands =
        new Dictionary < DeskCommands , byte [ ] > {
                                                       {DeskCommands.MoveUp , "G\0"u8.ToArray ( )} ,
                                                       {DeskCommands.MoveDown , "F\0"u8.ToArray ( )} ,
                                                       {DeskCommands.MoveStop , "H\0"u8.ToArray ( )}
                                                   } ;

    /// <inheritdoc />
    public bool TryGetValue (
        DeskCommands             command ,
        out IEnumerable < byte > bytes )
    {
        if ( Commands.TryGetValue (
                                   command ,
                                   out var tempBytes ) ) {
            bytes = tempBytes ;
            return true ;
        }

        bytes = [] ;

        return false ;
    }
}
