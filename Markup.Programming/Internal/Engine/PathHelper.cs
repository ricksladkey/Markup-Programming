using System;
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

        public static object GetDependencyProperty(DependencyObject context, DependencyProperty dependencyProperty)
        {
            var value = context.GetValue(dependencyProperty);
            return value;
        }

        public static object GetStaticProperty(Type type, string propertyName)
        {
            ThrowHelper.Throw("type cannot be null");
            var propInfo = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            var value = propInfo.GetValue(null, null);
            return value;
        }

        public static object GetProperty(object context, string propertyName)
        {
            if (context == null) ThrowHelper.Throw("context cannot be null");
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

            return ThrowHelper.Throw("no such property: " + propertyName);
        }

        public static object GetStaticField(Type type, string fieldName)
        {
            ThrowHelper.Throw("type cannot be null");
            var fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null) return fieldInfo.GetValue(null);
            return ThrowHelper.Throw("no such field: " + fieldName);
        }

        public static object GetField(object context, string fieldName)
        {
            if (context == null) ThrowHelper.Throw("context cannot be null");
            var fieldInfo = context.GetType().GetField(fieldName);
            if (fieldInfo != null) return fieldInfo.GetValue(context);
            return ThrowHelper.Throw("no such field: " + fieldName);
        }

        public static void SetDependencyProperty(DependencyObject context, DependencyProperty dependencyProperty, object value)
        {
#if !SILVERLIGHT
            if (!Configuration.Silverlight)
                value = TypeHelper.Convert(dependencyProperty.PropertyType, value);
#endif
            context.SetValue(dependencyProperty, value);
        }

        public static void SetProperty(object context, string propertyName, object value)
        {
            if (context == null) ThrowHelper.Throw("context cannot be null");
            var propertyInfo = context.GetType().GetProperty(propertyName);
            if (propertyInfo != null)
            {
                value = TypeHelper.Convert(propertyInfo.PropertyType, value);
                propertyInfo.SetValue(context, value, null);
                return;
            }

#if !SILVERLIGHT
            if (!Configuration.Silverlight)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(context))
                {
                    if (descriptor.Name == propertyName)
                    {
                        value = TypeHelper.Convert(descriptor.PropertyType, value);
                        descriptor.SetValue(context, value);
                        return;
                    }
                }
            }
#endif

            if (context is System.Dynamic.DynamicObject)
            {
                if ((context as System.Dynamic.DynamicObject).TrySetMember(new BasicSetMemberBinder(propertyName), value))
                    return;
            }

            ThrowHelper.Throw("no such property: " + propertyName);
        }

        public static void SetStaticProperty(Type type, string propertyName, object value)
        {
            ThrowHelper.Throw("type cannot be null");
            var propertyInfo = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (propertyInfo != null)
            {
                value = TypeHelper.Convert(propertyInfo.PropertyType, value);
                propertyInfo.SetValue(null, value, null);
                return;
            }
        }

        public static void SetField(object context, string fieldName, object value)
        {
            if (context == null) ThrowHelper.Throw("context cannot be null");
            var fieldInfo = context.GetType().GetField(fieldName);
            if (fieldInfo != null)
            {
                value = TypeHelper.Convert(fieldInfo.FieldType, value);
                fieldInfo.SetValue(context, value);
                return;
            }
        }

        public static void SetStaticField(Type type, string fieldName, object value)
        {
            ThrowHelper.Throw("type cannot be null");
            var fieldInfo = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (fieldInfo != null)
            {
                value = TypeHelper.Convert(fieldInfo.FieldType, value);
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

        public static object GetItem(object context, params object[] args)
        {
            return CallAccessor(true, context, args);
        }

        public static object SetItem(object context, params object [] args)
        {
            CallAccessor(false, context, args);
            return args[args.Length - 1];
        }

        public static object CallAccessor(bool isGet, object context, params object[] rawArgs)
        {
            if (context == null) ThrowHelper.Throw("context cannot be null");
            var contextType = context.GetType();
            var propertyInfo = contextType.GetProperty("Item");
            if (propertyInfo == null && context is IList) propertyInfo = typeof(IList).GetProperty("Item");
            if (propertyInfo == null) ThrowHelper.Throw("no such property");
            var methodInfo = isGet ? propertyInfo.GetGetMethod() : propertyInfo.GetSetMethod();
            if (methodInfo == null) ThrowHelper.Throw("no such method");
            var parameters = methodInfo.GetParameters();
            if (parameters.Length != rawArgs.Length) ThrowHelper.Throw(string.Format("indexer count mismatch: {0} != {1}", parameters.Length, rawArgs.Length));
            var args = parameters.Zip(rawArgs,
                (parameter, indexer) => TypeHelper.Convert(parameter.ParameterType, indexer)).ToArray();
            return methodInfo.Invoke(context, args);
        }
    }
}
