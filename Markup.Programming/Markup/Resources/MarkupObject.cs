using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows;
using Markup.Programming.Core;
using System.Diagnostics;

namespace Markup.Programming
{
    /// <summary>
    /// A MarkupObject is a dynamic object that can be defined and
    /// and used entirely in resources.
    /// </summary>

#if !SILVERLIGHT && !EMULATE_SILVERLIGHT

    [ContentProperty("Properties")]
#if DEBUG
    [DebuggerDisplay("Properties = {Properties.Count}"), DebuggerTypeProxy(typeof(DictionaryDebugView))]
#endif
    public class MarkupObject : ResourceObject, INotifyPropertyChanged, IProvideProperties, ICustomTypeDescriptor
    {
        public MarkupObject()
        {
            Properties = new PropertyCollection();
            Provider = new PropertyInfoProvider(this, this.GetType());
            propertyStore = new Dictionary<string, object>();
        }

        public override object Value
        {
            get { return this; }
            set { throw new NotImplementedException(); }
        }

        // This looks unusual but it hooks all the gets below.
        private PropertyInfoProvider provider;
        private PropertyInfoProvider Provider
        {
            get { TryToAttach(); return provider; }
            set { provider = value; }
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
            if (propertyInfo == null) return "MarkupObject: [Initializing]";
            var text = propertyInfo.Length == 0 ? "" :
                propertyInfo.Skip(1).Aggregate(propertyInfo[0].Name, (current, next) => current + ", " + next.Name);
            return string.Format("MarkupObject: " + text);
        }

    #region IProvidePropertyInfo Members

        public IEnumerable<NameTypePair> PropertyInfo
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

        public AttributeCollection GetAttributes() { return Provider.GetAttributes(); }
        public string GetClassName() { return Provider.GetClassName(); }
        public string GetComponentName() { return Provider.GetComponentName(); }
        public TypeConverter GetConverter() { return Provider.GetConverter(); }
        public PropertyDescriptor GetDefaultProperty() { return Provider.GetDefaultProperty(); }
        public object GetEditor(System.Type editorBaseType) { return Provider.GetEditor(editorBaseType); }
        public EventDescriptor GetDefaultEvent() { return Provider.GetDefaultEvent(); }
        public EventDescriptorCollection GetEvents(System.Attribute[] attributes) { return Provider.GetEvents(attributes); }
        public EventDescriptorCollection GetEvents() { return Provider.GetEvents(); }
        public object GetPropertyOwner(PropertyDescriptor pd) { return Provider.GetPropertyOwner(pd); }
        public PropertyDescriptorCollection GetProperties(System.Attribute[] attributes) { return Provider.GetProperties(attributes); }
        public PropertyDescriptorCollection GetProperties() { return Provider.GetProperties(); }

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
                value = TypeHelper.Convert(type, value);
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
    public class MarkupObject : ResourceObject, IProvideProperties
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
            value = DynamicTypeHelper.CreateObject(ref dynamicType, pairs);
        }

        public IEnumerable<NameTypePair> PropertyInfo
        {
            get { CheckEvaluate(); return value.PropertyInfo; }
        }

        public object this[string propertyName]
        {
            get { CheckEvaluate(); return this.value[propertyName]; }
            set { CheckEvaluate(); this.value[propertyName] = value; }
        }
    }

#endif

}
