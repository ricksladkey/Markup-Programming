using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Markup.Programming.Core
{
#if !SILVERLIGHT
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
                DynamicProperties = new NameTypePair[0],
                PropertyStore = new Dictionary<string, object>(),
            };

        private TypeDescriptorImplementor implementor;
        public DynamicObject() { implementor = new TypeDescriptorImplementor(this, this.GetType()); }

        public AttributeCollection GetAttributes() { return implementor.GetAttributes(); }
        public string GetClassName() { return implementor.GetClassName(); }
        public string GetComponentName() { return implementor.GetComponentName(); }
        public TypeConverter GetConverter() { return implementor.GetConverter(); }
        public PropertyDescriptor GetDefaultProperty() { return implementor.GetDefaultProperty(); }
        public object GetEditor(System.Type editorBaseType) { return implementor.GetEditor(editorBaseType); }
        public EventDescriptor GetDefaultEvent() { return implementor.GetDefaultEvent(); }
        public EventDescriptorCollection GetEvents(System.Attribute[] attributes) { return implementor.GetEvents(attributes); }
        public EventDescriptorCollection GetEvents() { return implementor.GetEvents(); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return implementor.GetPropertyOwner(pd); }
        public PropertyDescriptorCollection GetProperties(System.Attribute[] attributes) { return implementor.GetProperties(attributes); }
        public PropertyDescriptorCollection GetProperties() { return implementor.GetProperties(); }

        public override string ToString()
        {
            return DynamicHelper.ToString(this, "Object");
        }
    }
#endif
}
