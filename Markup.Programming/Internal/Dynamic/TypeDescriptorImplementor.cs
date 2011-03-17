using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Markup.Programming.Core
{
#if !SILVERLIGHT
    /// <summary>
    /// A TypeDescriptorImplementor implements the hard labor of
    /// ICustomTypeDescriptor so that client class can just
    /// create a provider and then forward all all the
    /// ICustomTypeDescriptor interface methods to the
    /// provider.  It uses the bridge pattern.
    /// </summary>
    public class TypeDescriptorImplementor : ICustomTypeDescriptor
    {
        public TypeDescriptorImplementor(IDynamicObject component, Type componentType)
        {
            Component = component;
            ComponentType = componentType;
        }

        public IDynamicObject Component { get; private set; }
        public Type ComponentType { get; private set; }

        public AttributeCollection GetAttributes() { return TypeDescriptor.GetAttributes(Component, true); }
        public string GetClassName() { return TypeDescriptor.GetClassName(Component, true); }
        public string GetComponentName() { return TypeDescriptor.GetComponentName(Component); }
        public TypeConverter GetConverter() { return TypeDescriptor.GetConverter(Component, true); }
        public PropertyDescriptor GetDefaultProperty() { return TypeDescriptor.GetDefaultProperty(Component, true); }
        public object GetEditor(Type editorBaseType) { return TypeDescriptor.GetEditor(Component, editorBaseType, true); }
        public EventDescriptor GetDefaultEvent() { return TypeDescriptor.GetDefaultEvent(Component, true); }
        public EventDescriptorCollection GetEvents(Attribute[] attributes) { return TypeDescriptor.GetEvents(Component, attributes, true); }
        public EventDescriptorCollection GetEvents() { return TypeDescriptor.GetEvents(Component, true); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return Component; }
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) { return GetProperties(); }

        public PropertyDescriptorCollection GetProperties()
        {
            var properties = Component.DynamicProperties.Select(pair => new BasicPropertyDescriptor(pair, ComponentType)).ToArray();
            return new PropertyDescriptorCollection(properties);
        }
    }

    public class BasicPropertyDescriptor : PropertyDescriptor
    {
        private Type propertyType;
        private Type componentType;

        public BasicPropertyDescriptor(NameTypePair pair, Type componentType)
            : base(pair.Name, new Attribute[] { })
        {
            this.propertyType = pair.Type;
            this.componentType = componentType;
        }

        public override bool CanResetValue(object component) { return true; }
        public override Type ComponentType { get { return componentType; } }
        public override object GetValue(object component) { return (component as IDynamicObject)[Name]; }
        public override bool IsReadOnly { get { return false; } }
        public override Type PropertyType { get { return propertyType; } }
        public override void ResetValue(object component) { SetValue(component, null); }
        public override void SetValue(object component, object value) { (component as IDynamicObject)[Name] = value; }
        public override bool ShouldSerializeValue(object component) { return true; }
    }
#endif
}
