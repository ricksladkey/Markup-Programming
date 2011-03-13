using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Markup.Programming.Core;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The DynamicObjectBase class is a bare-bones class that implements
    /// just enough base functionality to service dynamic properties
    /// and property change notification.
    /// </summary>
#if DEBUG
    [DebuggerDisplay("Properties = {PropertyStore.Count}"), DebuggerTypeProxy(typeof(PropertyInfoDebugView))]
#endif
    public class DynamicObjectBase : INotifyPropertyChanged, IProvideProperties
    {
        public Dictionary<string, object> PropertyStore { get; set; }
        public IEnumerable<NameTypePair> PropertyInfo { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public object this[string propertyName]
        {
            get { return PropertyStore[propertyName]; }
            set { PropertyStore[propertyName] = value; OnPropertyChanged(propertyName); }
        }
    }
}
