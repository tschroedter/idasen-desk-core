using System.Globalization ;
using System.Runtime.CompilerServices ;
using System.Text ;
using JetBrains.Annotations ;

namespace Idasen.BluetoothLE.Core ;

/// <summary>
///     Common guard helpers for argument validation.
/// </summary>
public static class Guard
{
    private const string ValueCannotBeNullOrEmpty = "Value cannot be null or empty" ;
    private const string ValueCannotBeWhitespace  = "Value cannot be null, empty or whitespace" ;

    private static readonly CompositeFormat ValueCannotBeNullOrEmptyFormat =
        CompositeFormat.Parse ( ValueCannotBeNullOrEmpty ) ;

    private static readonly CompositeFormat ValueCannotBeWhitespaceFormat =
        CompositeFormat.Parse ( ValueCannotBeWhitespace ) ;

    /// <summary>
    ///     Ensures the parameter is a non-null, non-empty, non-whitespace string.
    /// </summary>
    [ UsedImplicitly ]
    [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
    public static void ArgumentNotEmptyOrWhitespace ( object parameter ,
                                                      string parameterName )
    {
        ArgumentNotNullOrEmpty ( parameter ,
                                 parameterName ) ;

        if ( ! ( parameter is string text ) ||
             ! string.IsNullOrWhiteSpace ( text ) )
            return ;

        var message = string.Format ( CultureInfo.InvariantCulture ,
                                      ValueCannotBeWhitespaceFormat ) ;

        throw new ArgumentException ( message ,
                                      parameterName ) ;
    }

    /// <summary>
    ///     Ensures the parameter is not null.
    /// </summary>
    [ UsedImplicitly ]
    [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
    public static void ArgumentNotNull ( object parameter ,
                                         string parameterName )
    {
        if ( parameter == null )
            throw new ArgumentNullException ( parameterName ) ;
    }

    /// <summary>
    ///     Ensures the parameter is a non-null, non-empty string.
    /// </summary>
    [ UsedImplicitly ]
    [ MethodImpl ( MethodImplOptions.AggressiveInlining ) ]
    public static void ArgumentNotNullOrEmpty ( object parameter ,
                                                string parameterName )
    {
        ArgumentNotNull ( parameter ,
                          parameterName ) ;

        if ( ! ( parameter is string text ) ||
             text.Length != 0 )
            return ;

        var message = string.Format ( CultureInfo.InvariantCulture ,
                                      ValueCannotBeNullOrEmptyFormat ) ;

        throw new ArgumentException ( message ,
                                      parameterName ) ;
    }
}