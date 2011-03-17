using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A MarkupObject is a dynamic object that can be defined and
    /// and used entirely in resources.
    /// </summary>

#if !SILVERLIGHT && !EMULATE_SILVERLIGHT

    [ContentProperty("Properties")]
#if DEBUG
    [DebuggerDisplay("Properties = {Properties.Count}"), DebuggerTypeProxy(typeof(DynamicObjectDebugView))]
#endif
    public class MarkupObject : ResourceObject, INotifyPropertyChanged, IDynamicObject, ICustomTypeDescriptor
    {
        public MarkupObject()
        {
            Properties = new PropertyCollection();
            Implementor = new TypeDescriptorImplementor(this, this.GetType());
            propertyStore = new Dictionary<string, object>();
        }

        public override object Value
        {
            get { return this; }
            set { throw new NotImplementedException(); }
        }

        // This looks unusual but it hooks all the gets below.
        private TypeDescriptorImplementor implementor;
        private TypeDescriptorImplementor Implementor
        {
            get { TryToAttach(); return implementor; }
            set { implementor = value; }
        }

        private Dictionary<string, object> propertyStore;
        private NameTypePair[] propertyInfo;

        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(MarkupObject), null);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DynamicHelper.ToString(this, "MarkupObject");
        }

    #region IDyamicObject Members

        public IEnumerable<NameTypePair> DynamicProperties
        {
            get { return propertyInfo; }
        }

        public object this[string propertyName]
        {
            get { TryToAttach(); return propertyStore[propertyName]; }
            set { TryToAttach(); propertyStore[propertyName] = value; OnPropertyChanged(propertyName); }
        }

    #endregion

    #region ICumstomTypeTypeDescriptor

        public AttributeCollection GetAttributes() { return Implementor.GetAttributes(); }
        public string GetClassName() { return Implementor.GetClassName(); }
        public string GetComponentName() { return Implementor.GetComponentName(); }
        public TypeConverter GetConverter() { return Implementor.GetConverter(); }
        public PropertyDescriptor GetDefaultProperty() { return Implementor.GetDefaultProperty(); }
        public object GetEditor(System.Type editorBaseType) { return Implementor.GetEditor(editorBaseType); }
        public EventDescriptor GetDefaultEvent() { return Implementor.GetDefaultEvent(); }
        public EventDescriptorCollection GetEvents(System.Attribute[] attributes) { return Implementor.GetEvents(attributes); }
        public EventDescriptorCollection GetEvents() { return Implementor.GetEvents(); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return Implementor.GetPropertyOwner(pd); }
        public PropertyDescriptorCollection GetProperties(System.Attribute[] attributes) { return Implementor.GetProperties(attributes); }
        public PropertyDescriptorCollection GetProperties() { return Implementor.GetProperties(); }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(PropertiesProperty);
        }

        public override void EndInit()
        {
            base.TryToAttach();
        }

        protected override void TryToAttach()
        {
            base.TryToAttach();
            if (!Evaluated) new Engine().With(this, engine => EvaluateProperties(engine));
        }

        public bool Evaluated { get; set; }

        private void EvaluateProperties(Engine engine)
        {
            if (Evaluated) return;
            foreach (var property in Properties)
            {
                var value = property.Evaluate(engine);
                var type = property.Type;
                value = TypeHelper.Convert(value, type);
                propertyStore.Add(property.PropertyName, value);
            }
            propertyInfo = Properties.Select(property => GetPair(property)).ToArray();
            Evaluated = true;
        }

        private NameTypePair GetPair(Property property)
        {
            var name = property.PropertyName;
            var type = property.Type ?? typeof(object);
            return new NameTypePair(name, type);
        }
    }

#else

    [ContentProperty("Properties")]
    public class MarkupObject : ResourceObject, IDynamicObject
    {
        public MarkupObject()
        {
            Properties = new PropertyCollection();
        }

        public PropertyCollection Properties { get; set; }

        private bool evaluated;
        private DynamicObjectBase value;

        public override object Value
        {
            get { CheckEvaluate();  return value; }
            set { throw new NotImplementedException(); }
        }

        public override void EndInit()
        {
            CheckEvaluate();
        }

        private void CheckEvaluate()
        {
            if (evaluated) return;
            new Engine().With(this, engine => EvaluateProperties(engine));
            evaluated = true;
        }

        private void EvaluateProperties(Engine engine)
        {
            var pairs = Properties.Select(property =>
                new NameValuePair(property.PropertyName, property.Evaluate(engine))).ToArray();
            Type dynamicType = null;
            value = DynamicHelper.CreateObject(ref dynamicType, pairs);
        }

        public IEnumerable<NameTypePair> DynamicProperties
        {
            get { CheckEvaluate(); return value.DynamicProperties; }
        }

        public object this[string propertyName]
        {
            get { CheckEvaluate(); return this.value[propertyName]; }
            set { CheckEvaluate(); this.value[propertyName] = value; }
        }
    }

#endif

}
