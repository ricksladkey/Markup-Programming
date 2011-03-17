using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The TypeHelper class is used to perform type operations
    /// using whatever mechanisms are appropriate for the
    /// current platform.
    /// </summary>
    public static class TypeHelper
    {
        public static object CreateInstance(Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public static bool IsClassObject(object value)
        {
            return value != null && value.GetType().IsClass;
        }

        public static bool ConvertToBool(object value)
        {
            return (bool)Convert(value, typeof(bool));
        }

        public static Type ConvertToType(string value)
        {
            return Convert(value, typeof(Type)) as Type;
        }

        public static object Convert(object value, Type type)
        {
            if (type == typeof(Type) && value is string) return ParseTypeName(value as string);
            if (type != null && value != null && value.GetType() != type)
            {
#if !SILVERLIGHT
                if (!Configuration.Silverlight)
                {
                    var converter = TypeDescriptor.GetConverter(type);
                    if (converter.CanConvertFrom(value.GetType()))
                        value = converter.ConvertFrom(value);
                }
                else
                    value = GenericConvert(type, value);
#else
                value = GenericConvert(type, value);
#endif
            }
            return value;
        }

        private static Type ParseTypeName(string typeName)
        {
            var app = Application.Current;
            if (typeName.IndexOf(",") != -1) return Type.GetType(typeName, false);
            var appType = app.GetType();
            if (typeName.IndexOf(".") < 0) typeName = appType.Namespace + "." + typeName;
            var type = appType.Assembly.GetType(typeName, false);
            if (type == null) type = Type.GetType(typeName, false);
            return type;
        }

        private static object GenericConvert(Type type, object value)
        {
            if (value is IConvertible) return System.Convert.ChangeType(value, type, null);
            return value;
        }

        public static Type DeduceType(ICollection<object> values)
        {
            // Try to find a common type among the elements.
            var result = values.Aggregate(null as Type,
                (current, next) => GetCommonType(current, next != null ? next.GetType() : typeof(object)));

            if (result == typeof(object))
            {
                // Try again with interfaces.
                var interfaceSets = values.Select(item => item != null ? item.GetType().GetInterfaces() : null);
                foreach (var commonInterface in CommonInterfaces)
                {
                    if (interfaceSets.All(interfaceSet =>
                        interfaceSet == null || interfaceSet.Contains(commonInterface)))
                    {
                        result = commonInterface;
                        break;
                    }
                }
            }

            return result ?? typeof(object);
        }

        public static IEnumerable<Type> CommonInterfaces { get { return commonInterfaces; } }
        private static Type[] commonInterfaces =
        {
            typeof(IList),
            typeof(ICollection),
            typeof(IEnumerable),
        };

        public static Type GetCommonType(Type type1, Type type2)
        {
            if (type1 == null) return type2;
            if (type1.IsAssignableFrom(type2)) return type1;
            if (type2.IsAssignableFrom(type1)) return type2;
            return typeof(object);
        }

        public static object CreateArray(IList<object> collection)
        {
            return CreateArray(collection, null);
        }

        public static object CreateArray(IList<object> collection, Type type)
        {
            var newType = type ?? DeduceType(collection);
            var array = Array.CreateInstance(newType, collection.Count);
            for (int i = 0; i < collection.Count; i++) array.SetValue(collection[i], i);
            return array;
        }

        public static IList CreateCollection(IList<object> collection)
        {
            return CreateCollection(collection, null, null);
        }

        public static IList CreateCollection(IList<object> collection, Type type, Type typeArgument)
        {
            if ((type == null || type.IsGenericType) && typeArgument == null)
                typeArgument = DeduceType(collection);
#if !SILVERLIGHT
            if (!Configuration.Silverlight)
            {
                if (type == null && typeArgument == typeof(DynamicObject) && collection.Count > 0)
                    return CreateDynamicCollection(collection);
            }
#endif
            if (type == null) type = typeof(ObservableCollection<>);
            var collectionType = type.IsGenericType ? type.MakeGenericType(typeArgument) : type;
            var newCollection = Activator.CreateInstance(collectionType) as IList;
            foreach (var item in collection) newCollection.Add(item);
            return newCollection;
        }

#if !SILVERLIGHT
        private static IList CreateDynamicCollection(IList<object> collection)
        {
            var representativeItem = collection[0] as DynamicObject;
            var newCollection = new DynamicObservableCollection<DynamicObject>(representativeItem);
            foreach (var item in collection) newCollection.Add(item as DynamicObject);
            return newCollection;
        }
#endif

        public static Type ResolvePartialType(string typeName)
        {
            if (typeName.Contains('.')) return SearchAssembliesForType(typeName);
            foreach (var prefix in Namespaces)
            {
                var type = SearchAssembliesForType(prefix + "." + typeName);
                if (type != null) return type;
            }
            return null;
        }

        private static Type SearchAssembliesForType(string typeName)
        {
            foreach (var assembly in Assemblies)
            {
                var type = assembly.GetType(typeName, false);
                if (type != null) return type;
            }
            return null;
        }

        private static IEnumerable<string> Namespaces
        {
            get { return namespaces; }
        }

        private static string[] namespaces =
        {
            "System",
            "System.Windows",
            "System.Windows.Controls",
            "System.COllections.Generic",
            "System.Collections",
        };
 
#if !SILVERLIGHT
        private static IEnumerable<Assembly> Assemblies
        {
            get { return AppDomain.CurrentDomain.GetAssemblies(); }
        }
#else
        private static Type[] coreTypes =
        {
            typeof(Array), // mscorlib
            typeof(System.Diagnostics.Debug), // System
            typeof(System.Collections.Generic.HashSet<>), // System.Core
            typeof(System.Net.Sockets.Socket), // System.Net
            typeof(Window), // System.Windows
            typeof(System.Windows.Browser.BrowserInformation),
            typeof(System.Xml.XmlWriter), // System.Xml
#if INTERACTIVITY
            typeof(System.Windows.Interactivity.IAttachedObject), // portable
#endif
            typeof(IStatement), // Markup.Programming
        };
        private static Assembly[] assemblies = coreTypes.Select(type => type.Assembly).ToArray();
        private static IEnumerable<Assembly> Assemblies { get { return assemblies; } }
#endif
    }
}
