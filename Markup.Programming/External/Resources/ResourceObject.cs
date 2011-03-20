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
    /// A ResourceObject is a dynamic object that can be defined and
    /// and used entirely in resources.
    /// </summary>

#if !SILVERLIGHT && !EMULATE_SILVERLIGHT

    [ContentProperty("Properties")]
#if DEBUG
    [DebuggerDisplay("Properties = {Properties.Count}"), DebuggerTypeProxy(typeof(DynamicObjectDebugView))]
#endif
    public class ResourceObject : LazyResourceComponent, INotifyPropertyChanged, IDynamicObject, ICustomTypeDescriptor
    {
        public ResourceObject()
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
            get { Initialize(); return implementor; }
            set { implementor = value; }
        }

        private Dictionary<string, object> propertyStore;
        private NameTypePair[] dynamicProperties;

        public PropertyCollection Properties
        {
            get { return (PropertyCollection)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(PropertyCollection), typeof(ResourceObject), null);

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return DynamicHelper.ToString(this, "ResourceObject");
        }

    #region IDyamicObject Members

        public IEnumerable<NameTypePair> DynamicProperties
        {
            get { return dynamicProperties; }
        }

        public object this[string propertyName]
        {
            get { Initialize(); return propertyStore[propertyName]; }
            set { Initialize(); propertyStore[propertyName] = value; OnPropertyChanged(propertyName); }
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

        protected override void OnInitialize(Engine engine)
        {
            foreach (var property in Properties)
            {
                var type = property.GetType(engine);
                var value = property.Evaluate(engine);
                value = TypeHelper.Convert(value, type);
                propertyStore.Add(property.Prop, value);
            }
            dynamicProperties = Properties.Select(property => GetPair(engine, property)).ToArray();
        }

        private NameTypePair GetPair(Engine engine, Property property)
        {
            var name = property.Prop;
            var type = property.GetType(engine) ?? typeof(object);
            return new NameTypePair(name, type);
        }
    }

#else

    [ContentProperty("Properties")]
    public class ResourceObject : LazyResourceComponent, IDynamicObject
    {
        private DynamicObjectBase value;

        public ResourceObject()
        {
            Properties = new PropertyCollection();
        }

        public PropertyCollection Properties { get; private set; }

        public override object Value
        {
            get { Initialize();  return value; }
            set { throw new NotImplementedException(); }
        }

        protected override void OnInitialize(Engine engine)
        {
            var pairs = Properties.Select(property =>
                new NameValuePair(property.Prop, property.Evaluate(engine))).ToArray();
            Type dynamicType = null;
            value = DynamicHelper.CreateObject(ref dynamicType, pairs);
        }

        public IEnumerable<NameTypePair> DynamicProperties
        {
            get { Initialize(); return value.DynamicProperties; }
        }

        public object this[string propertyName]
        {
            get { Initialize(); return this.value[propertyName]; }
            set { Initialize(); this.value[propertyName] = value; }
        }
    }

#endif

}
