namespace Idasen.BluetoothLE.Characteristics.Characteristics ;

using Aop.Aspects ;
using Autofac.Extras.DynamicProxy ;
using Common ;
using Interfaces.Characteristics ;

[ Intercept ( typeof ( LogAspect ) ) ]
public class DescriptionToUuid
    : SimpleDictionaryBase < string , Guid > ,
      IDescriptionToUuid ;
