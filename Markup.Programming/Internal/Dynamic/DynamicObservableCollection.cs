using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    public class DynamicObservableCollection<T> : ObservableCollection<T>, ITypedList where T : DynamicObject
    {
        private T representativeItem;

        public DynamicObservableCollection(T representativeItem)
        {
            this.representativeItem = representativeItem;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            var item = representativeItem;
            if (item == null && Count > 0) item = this[0];
            if (item == null) ThrowHelper.Throw("cannot find representative item");
            return item.GetProperties();
        }

        public string GetListName(PropertyDescriptor[] listAccessors) { throw new NotImplementedException(); }
    }
}
