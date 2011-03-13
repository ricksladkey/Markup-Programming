using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A DynamicTemplateObject implements a dynamic object using the
    /// ICustomTypeDescriptor interface which is supported by WPF.  Neither
    /// the interface not this class is supported by Silverlight.  See
    /// DynamicTypeHelper.CreateType for an alternative.
    /// </summary>
    public class DynamicObject : DynamicObjectBase, ICustomTypeDescriptor
    {
        public static DynamicObject Empty =
            new DynamicObject
            {
                PropertyInfo = new NameTypePair[0],
                PropertyStore = new Dictionary<string, object>(),
            };

        private PropertyInfoProvider provider;
        public DynamicObject() { provider = new PropertyInfoProvider(this, this.GetType()); }

        public AttributeCollection GetAttributes() { return provider.GetAttributes(); }
        public string GetClassName() { return provider.GetClassName(); }
        public string GetComponentName() { return provider.GetComponentName(); }
        public TypeConverter GetConverter() { return provider.GetConverter(); }
        public PropertyDescriptor GetDefaultProperty() { return provider.GetDefaultProperty(); }
        public object GetEditor(System.Type editorBaseType) { return provider.GetEditor(editorBaseType); }
        public EventDescriptor GetDefaultEvent() { return provider.GetDefaultEvent(); }
        public EventDescriptorCollection GetEvents(System.Attribute[] attributes) { return provider.GetEvents(attributes); }
        public EventDescriptorCollection GetEvents() { return provider.GetEvents(); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return provider.GetPropertyOwner(pd); }
        public PropertyDescriptorCollection GetProperties(System.Attribute[] attributes) { return provider.GetProperties(attributes); }
        public PropertyDescriptorCollection GetProperties() { return provider.GetProperties(); }

        public override string ToString()
        {
            return DynamicObjectHelper.ToString(this, "Object");
        }
    }
}
