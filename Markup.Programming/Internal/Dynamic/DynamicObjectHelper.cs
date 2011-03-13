using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The DynamicObjectHelper is used to create dynamic types and objects.
    /// Unfortunately practically no method for dynamic properties works on
    /// Silverlight and so we must resort to literally emitting
    /// intermediate language and creating new types.  Luckily by using
    /// a base class this can be confined to just a few opcodes per
    /// getter and setter.
    /// </summary>
    public static class DynamicObjectHelper
    {
        private static MethodInfo getItemMethodInfo = typeof(DynamicObjectBase).GetProperty("Item").GetGetMethod();
        private static MethodInfo setItemMethodInfo = typeof(DynamicObjectBase).GetProperty("Item").GetSetMethod();

        public static DynamicObjectBase CreateObject(ref Type dynamicType, IEnumerable<NameValuePair> pairs)
        {
            var propertyInfo = pairs.Select(pair => new NameTypePair(pair.Name, typeof(object))).ToArray();
            if (!Configuration.Silverlight) return CreateObjectWPF(ref dynamicType, pairs, propertyInfo);
            return CreateObjectSilverlight(ref dynamicType, pairs, propertyInfo);
        }

        public static DynamicObjectBase CreateObjectWPF(ref Type dynamicType, IEnumerable<NameValuePair> pairs, IEnumerable<NameTypePair> propertyInfo)
        {
            dynamicType = null;
            var propertyStore = new Dictionary<string, object>();
            foreach (var pair in pairs) propertyStore.Add(pair.Name, pair.Value);
#if !SILVERLIGHT
            if (Configuration.Silverlight) return null;
            var instance = new DynamicObject { PropertyInfo = propertyInfo, PropertyStore = propertyStore };
            return instance;
#else
            return null;
#endif
        }

        public static DynamicObjectBase CreateObjectSilverlight(ref Type dynamicType, IEnumerable<NameValuePair> pairs, IEnumerable<NameTypePair> propertyInfo)
        {
            if (dynamicType == null) dynamicType = CreateType(propertyInfo);
            var instance = Activator.CreateInstance(dynamicType) as DynamicObjectBase;
            instance.PropertyInfo = propertyInfo;
            instance.PropertyStore = new Dictionary<string, object>();
            foreach (var pair in pairs) instance[pair.Name] = pair.Value;
            return instance;
        }

        public static Type CreateType(IEnumerable<NameTypePair> propertyInfo)
        {
            var assemblyName = new AssemblyName { Name = "DynamicAssembly" };
            var assemblyBuilder =
               Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            var typeBuilder =
               moduleBuilder.DefineType("DynamicObject", TypeAttributes.Public, typeof(DynamicObjectBase));
            var constructor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
            foreach (var pair in propertyInfo)
            {
                var propertyName = pair.Name;
                var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault,
                    typeof(object), Type.EmptyTypes);
                var getMethodBuilder = typeBuilder.DefineMethod("get_" + propertyName,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    typeof(object), Type.EmptyTypes);
                var setMethodBuilder = typeBuilder.DefineMethod("set_" + propertyName,
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    null, new Type[] { typeof(object) });
                {
                    var il = getMethodBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, propertyName);
                    il.EmitCall(OpCodes.Call, getItemMethodInfo, null);
                    il.Emit(OpCodes.Ret);
                }
                {
                    var il = setMethodBuilder.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, propertyName);
                    il.Emit(OpCodes.Ldarg_1);
                    il.EmitCall(OpCodes.Call, setItemMethodInfo, null);
                    il.Emit(OpCodes.Ret);
                }
                propertyBuilder.SetGetMethod(getMethodBuilder);
                propertyBuilder.SetSetMethod(setMethodBuilder);
            }
            return typeBuilder.CreateType();
        }

        public static string ToString(IProvideProperties component, string name)
        {
            var propertyInfo = component.PropertyInfo;
            if (propertyInfo == null) return name + ": [Initializing]";
            var text = propertyInfo.Count() == 0 ? "" :
                propertyInfo.Skip(1).Aggregate(propertyInfo.First().Name, (current, next) => current + ", " + next.Name);
            return name + ": " + text;
        }
    }
}
