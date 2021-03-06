﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Expression = System.Linq.Expressions.Expression;
    
namespace Markup.Programming.Core
{
    /// <summary>
    /// The PathHelper class is used to set properties and fields using
    /// whatever mechanism is supported by the current platform.  Where
    /// supported this includes CLR properties, System.ComponentModel,
    /// and System.Dynamic.
    /// </summary>
    public class PathHelper
    {
        private class BasicGetMemberBinder : GetMemberBinder
        {
            public BasicGetMemberBinder(string propertyName)
                : base(propertyName, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject context, DynamicMetaObject errorSuggestion)
            {
                return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }), new Expression[] { Expression.Constant("no such property") }), ReturnType), BindingRestrictions.Empty);
            }
        }

        private class BasicSetMemberBinder : SetMemberBinder
        {
            public BasicSetMemberBinder(string propertyName)
                : base(propertyName, false)
            {
            }

            public override DynamicMetaObject FallbackSetMember(DynamicMetaObject context, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
            {
                return errorSuggestion ?? new DynamicMetaObject(Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) }), new Expression[] { Expression.Constant("no such property") }), ReturnType), BindingRestrictions.Empty);
            }
        }

        public static object GetDependencyProperty(Engine engine, DependencyObject context, DependencyProperty dependencyProperty)
        {
            var value = context.GetValue(dependencyProperty);
            return value;
        }

        public static object GetStaticProperty(Engine engine, Type type, string propertyName)
        {
            if (type == null) engine.Throw("type cannot be null for property: " + propertyName);
            if (type.IsEnum) return GetEnumValue(engine, type, propertyName);
            var propInfo = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var value = propInfo.GetValue(null, null);
            return value;
        }

        public static object GetEnumValue(Engine engine, Type type, string enumName)
        {
            if (!type.IsEnum) engine.Throw("not an enum: " + type.FullName);
            try
            {
                return Enum.Parse(type, enumName, false);
            }
            catch
            {
            }
            return engine.Throw("enum value not found: " + enumName);
        }

        public static Type GetPropertyType(Engine engine, object context, string propertyName)
        {
            if (context == null) engine.Throw("context cannot be null for property: " + propertyName);
            var propertyInfo = context.GetType().GetProperty(propertyName);
            if (propertyInfo != null) return propertyInfo.PropertyType;

#if !SILVERLIGHT
            if (!Configuration.Silverlight)
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(context))
                    if (descriptor.Name == propertyName) return descriptor.PropertyType;
#endif

            if (context is System.Dynamic.DynamicObject)
            {
                var value = GetProperty(engine, context, propertyName);
                return value != null ? value.GetType() : typeof(object);
            }

#if SILVERLIGHT
            if (context is IDynamicObject)
            {
                var dynamicObject = context as IDynamicObject;
                foreach (var property in dynamicObject.DynamicProperties)
                    if (property.Name == propertyName) return property.Type;
            }
#endif

            return engine.Throw("no such property: " + propertyName) as Type;
        }

        public static object GetProperty(Engine engine, object context, string propertyName)
        {
            if (context == null) engine.Throw("context cannot be null for property: " + propertyName);
            object value = null;
            var propertyInfo = context.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(context, null);
                return value;
            }

#if !SILVERLIGHT
            if (!Configuration.Silverlight)
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(context))
                    if (descriptor.Name == propertyName) return descriptor.GetValue(context);
#endif

            if (context is System.Dynamic.DynamicObject)
                if ((context as System.Dynamic.DynamicObject).TryGetMember(new BasicGetMemberBinder(propertyName), out value)) return value;

#if SILVERLIGHT
            if (context is IDynamicObject)
            {
                var dynamicObject = context as IDynamicObject;
                foreach (var property in dynamicObject.DynamicProperties)
                    if (property.Name == propertyName) return dynamicObject[propertyName];
            }
#endif

            return engine.Throw("no such property: " + propertyName);
        }

        public static object GetStaticField(Engine engine, Type type, string fieldName)
        {
            engine.Throw("type cannot be null for field: " + fieldName);
            var fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null) return fieldInfo.GetValue(null);
            return engine.Throw("no such field: " + fieldName);
        }

        public static object GetField(Engine engine, object context, string fieldName)
        {
            if (context == null) engine.Throw("context cannot be null for field: " + fieldName);
            var fieldInfo = context.GetType().GetField(fieldName);
            if (fieldInfo != null) return fieldInfo.GetValue(context);
            return engine.Throw("no such field: " + fieldName);
        }

        public static void SetDependencyProperty(Engine engine, DependencyObject context, DependencyProperty dependencyProperty, object value)
        {
#if !SILVERLIGHT
            if (!Configuration.Silverlight)
                value = TypeHelper.Convert(value, dependencyProperty.PropertyType);
#endif
            context.SetValue(dependencyProperty, value);
        }

        public static object SetProperty(Engine engine, object context, string propertyName, object value)
        {
            if (context == null) engine.Throw("context cannot be null for property: " + propertyName);
            var propertyInfo = context.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                value = TypeHelper.Convert(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(context, value, null);
                return value;
            }

#if !SILVERLIGHT
            if (!Configuration.Silverlight)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(context))
                {
                    if (descriptor.Name == propertyName)
                    {
                        value = TypeHelper.Convert(value, descriptor.PropertyType);
                        descriptor.SetValue(context, value);
                        return value;
                    }
                }
            }
#endif

            if (context is System.Dynamic.DynamicObject)
            {
                if ((context as System.Dynamic.DynamicObject).TrySetMember(new BasicSetMemberBinder(propertyName), value))
                    return value;
            }

#if SILVERLIGHT
            if (context is IDynamicObject)
            {
                var dynamicObject = context as IDynamicObject;
                foreach (var property in dynamicObject.DynamicProperties)
                    if (property.Name == propertyName)
                    {
                        dynamicObject[propertyName] = value;
                        return value;
                    }
            }
#endif

            return engine.Throw("no such property: " + propertyName);
        }

        public static void SetStaticProperty(Engine engine, Type type, string propertyName, object value)
        {
            if (type == null) engine.Throw("type cannot be null for property: " + propertyName);
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (propertyInfo != null)
            {
                value = TypeHelper.Convert(value, propertyInfo.PropertyType);
                propertyInfo.SetValue(null, value, null);
                return;
            }
        }

        public static void SetField(Engine engine, object context, string fieldName, object value)
        {
            if (context == null) engine.Throw("context cannot be null for field: " + fieldName);
            var fieldInfo = context.GetType().GetField(fieldName);
            if (fieldInfo != null)
            {
                value = TypeHelper.Convert(value, fieldInfo.FieldType);
                fieldInfo.SetValue(context, value);
                return;
            }
        }

        public static void SetStaticField(Engine engine, Type type, string fieldName, object value)
        {
            engine.Throw("type cannot be null for field: " + fieldName);
            var fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null)
            {
                value = TypeHelper.Convert(value, fieldInfo.FieldType);
                fieldInfo.SetValue(null, value);
                return;
            }
        }

        public static bool HasBinding(DependencyObject context, DependencyProperty property)
        {
#if !SILVERLIGHT
            if (!Configuration.Silverlight)
                return BindingOperations.GetBinding(context, property) != null;
#endif
            return false;

        }

        public static bool HasBindingOrValue(DependencyObject context, DependencyProperty property)
        {
            return HasBinding(context, property) || context.GetValue(property) != null;
        }

        public static bool HasBindingOrValue(DependencyObject context, DependencyProperty property, string path)
        {
            return HasBindingOrValue(context, property) || path != null;
        }

        public static object GetItem(Engine engine, object context, params object[] args)
        {
            return CallAccessor(engine, false, context, args);
        }

        public static object SetItem(Engine engine, object context, params object[] args)
        {
            CallAccessor(engine, true, context, args);
            return args[args.Length - 1];
        }

        public static object CallAccessor(Engine engine, bool isSet, object context, params object[] rawArgs)
        {
            if (context == null) engine.Throw("context cannot be null for item accessor");
            var contextType = context.GetType();
            var propertyInfo = contextType.GetProperty("Item");
            if (propertyInfo == null && context is IList) propertyInfo = typeof(IList).GetProperty("Item");
            if (propertyInfo == null) engine.Throw("no such property");
            var methodInfo = isSet ? propertyInfo.GetSetMethod() : propertyInfo.GetGetMethod();
            if (methodInfo == null) engine.Throw("no such method");
            var parameters = methodInfo.GetParameters();
            if (parameters.Length != rawArgs.Length)
                engine.Throw("indexer count mismatch: {0} != {1}", parameters.Length, rawArgs.Length);
            var args = parameters.Zip(rawArgs,
                (parameter, indexer) => TypeHelper.Convert(indexer, parameter.ParameterType)).ToArray();
            return methodInfo.Invoke(context, args);
        }
    }
}
