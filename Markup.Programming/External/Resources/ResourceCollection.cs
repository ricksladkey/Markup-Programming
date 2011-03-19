using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A ResourceCollection is a collection that implements
    /// INotifyCollectionChanged and is used for defining collections
    /// in resources.
    /// </summary>
    [ContentProperty("Items")]
    public class ResourceCollection : ResourceComponent, IList, INotifyCollectionChanged, INotifyPropertyChanged, ISupportInitialize
    {
        public ResourceCollection()
        {
            Items = new List<object>();
            Collection = new ObservableCollection<object>();
            Collection.CollectionChanged += new NotifyCollectionChangedEventHandler(ResourceCollection_CollectionChanged);
        }

        private ObservableCollection<object> Collection { get; set; }
        public List<object> Items { get; set; }

        public override void EndInit()
        {
            base.EndInit();
            foreach (var child in Items) Add(child);
        }

        #region INotifyCollectionChanged Members

        void  ResourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null) CollectionChanged(this, e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged("Count");
                    break;
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IList Members

        public int Add(object value) { Collection.Add(value); return Collection.Count - 1; }
        public void Clear() { Collection.Clear(); }
        public bool Contains(object value) { return Collection.Contains(value); }
        public int IndexOf(object value) { return Collection.IndexOf(value); }
        public void Insert(int index, object value) { Collection.Insert(index, value); }
        public bool IsFixedSize { get { return false; } }
        public bool IsReadOnly { get { return false; } }
        public void Remove(object value) { Collection.Remove(value); }
        public void RemoveAt(int index) { Collection.RemoveAt(index); }
        public object this[int index]
        {
            get { return Collection[index]; }
            set { Collection[index] = value; }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) { foreach (var item in Collection) array.SetValue(item, index++); }
        public int Count { get { return Collection.Count; } }
        public bool IsSynchronized { get { return false; } }
        public object SyncRoot { get { return syncRoot; } }
        private object syncRoot = new object();

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() { return Collection.GetEnumerator(); }

        #endregion
    }
}
