using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;
using System.ComponentModel;

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

        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Object), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(Object), null);

        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(Object), null);

        private Type dynamicType;

        protected override object OnEvaluate(Engine engine)
        {
            var pairs = Properties.Select(property =>
                new NameValuePair(property.PropertyName, property.Evaluate(engine))).ToArray();
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (type != null)
            {
                var target = Activator.CreateInstance(type);
                foreach (var pair in pairs) PathHelper.SetProperty(target, pair.Name, pair.Value);
                return target;
            }
            return DynamicHelper.CreateObject(ref dynamicType, pairs);
        }
    }
}
