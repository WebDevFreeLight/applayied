using System;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ZR.Common.DynamicApiSimple;

class ApiFeatureProvider : ControllerFeatureProvider
{
    protected override bool IsController(TypeInfo typeInfo)
    {
        Type type = typeInfo.AsType();
        // ????????????????????????
        if (!type.IsPublic || type.IsValueType || type.IsAbstract || type.IsGenericType || type.IsPrimitive || string.IsNullOrWhiteSpace(type.Namespace)) return false;

        // ???????IDynamicApiController(?),[DynamicApi](??)
        if ((!typeof(Controller).IsAssignableFrom(type) && typeof(ControllerBase).IsAssignableFrom(type)) || type.IsDefined(typeof(DynamicApiAttribute), true) || typeof(IDynamicApi).IsAssignableFrom(type))
        {
            // ?????????????????????,???????
            if (type.IsDefined(typeof(ApiExplorerSettingsAttribute), true) && type.GetCustomAttribute<ApiExplorerSettingsAttribute>(true).IgnoreApi)
            {
                return false;
            }
            return true;
        }
        return false;
    }
}

