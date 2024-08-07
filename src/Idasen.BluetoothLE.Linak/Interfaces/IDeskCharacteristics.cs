﻿using Idasen.BluetoothLE.Characteristics.Interfaces.Characteristics ;
using Idasen.BluetoothLE.Core.Interfaces.ServicesDiscovery ;

// ReSharper disable UnusedMemberInSuper.Global

namespace Idasen.BluetoothLE.Linak.Interfaces
{
    public interface IDeskCharacteristics
    {
        IGenericAccess       GenericAccess    { get ; }
        IGenericAttribute    GenericAttribute { get ; }
        IReferenceInput      ReferenceInput   { get ; }
        IReferenceOutput     ReferenceOutput  { get ; }
        IDpg                 Dpg              { get ; }
        IControl             Control          { get ; }
        IDeskCharacteristics Initialize ( IDevice device ) ;
        Task                 Refresh ( ) ;

        IDeskCharacteristics WithCharacteristics (
            DeskCharacteristicKey key ,
            ICharacteristicBase   characteristic ) ;
    }
}