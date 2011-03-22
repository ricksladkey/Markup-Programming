using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;
using System.ComponentModel;
using System.Collections;

namespace Markup.Programming
{
    /// <summary>
    /// An Object expression is a prototype for an object
    /// with specified properties and each properties value, all
    /// of which can be expressions.  Each time the TemplateObject
    /// is evaluated a new dynamic object is created that will
    /// contains the properties and values specified as well as
    /// implementing change notification.
    /// </summary>
    [ContentProperty("Properties")]
    public class Object : ExpressionBase
    {
        public Object()
        {
            Properties = new PropertyCollection();
        }

        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(Object), null);

        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(Object), null);

        private Type dynamicType;

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            var type = engine.EvaluateType(TypeProperty, Path, CodeTree);
            if (type == null)
            {
                var pairs = Properties.Select(property => new NameValuePair(property.PropertyName, property.Evaluate(engine)));
                return DynamicHelper.CreateObject(ref dynamicType, pairs.ToArray());
            }

            var target = Activator.CreateInstance(type);
            foreach (var property in Properties)
            {
                var value = property.Evaluate(engine);
                var propertyType = PathHelper.GetPropertyType(engine, target, property.PropertyName);
                if (typeof(IList).IsAssignableFrom(propertyType) && property.Value is Collection)
                {
                    var collection = PathHelper.GetProperty(engine, target, property.PropertyName) as IList;
                    foreach (var item in value as IEnumerable) collection.Add(item);
                }
                else if (typeof(IDictionary).IsAssignableFrom(propertyType) && property.Value is Collection)
                {
                    var dictionary = PathHelper.GetProperty(engine, target, property.PropertyName) as IDictionary;
                    foreach (DictionaryEntry entry in value as IEnumerable) dictionary.Add(entry.Key, entry.Value);
                }
                else
                    PathHelper.SetProperty(engine, target, property.PropertyName, value);
            }
            return target;
        }
    }
}
