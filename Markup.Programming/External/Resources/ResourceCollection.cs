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
    public class ResourceCollection : LazyResourceComponent, IList, INotifyCollectionChanged, INotifyPropertyChanged, ISupportInitialize
    {
        private object value;
        private ObservableCollection<object> collection;

        public ResourceCollection()
        {
            Items = new List<object>();
            collection = new ObservableCollection<object>();
            collection.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        public List<object> Items { get; set; }

        public override object Value
        {
            get { return this; }
            set { this.value = value; }
        }

        protected override void OnInitialize(Engine engine)
        {
            foreach (var item in GetItems()) Add(item);
        }

        private IList GetItems()
        {
            if (value == null) return Items;
            return new Engine().EvaluateFrame(this, engine => EvaluateItems(engine, value));
        }

        private IList EvaluateItems(Engine engine, object value)
        {
            var result = engine.EvaluateObject(value);
            if (!(result is IList)) engine.Throw("value does not evaluate to a collection");
            return result as IList;
        }

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { Initialize(); CollectionChangedInternal += value; }
            remove { Initialize(); CollectionChangedInternal -= value; }
        }

        private event NotifyCollectionChangedEventHandler CollectionChangedInternal;

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChangedInternal;
            if (handler != null) CollectionChangedInternal(this, e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    OnPropertyChanged("Count");
                    break;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged
        {
            add { Initialize(); PropertyChangedInternal += value; }
            remove { Initialize(); PropertyChangedInternal -= value; }
        }

        private event PropertyChangedEventHandler PropertyChangedInternal;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChangedInternal;
            if (handler != null) PropertyChangedInternal(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IList Members

        public int Add(object value) { Initialize(); collection.Add(value); return collection.Count - 1; }
        public void Clear() { Initialize(); collection.Clear(); }
        public bool Contains(object value) { Initialize(); return collection.Contains(value); }
        public int IndexOf(object value) { Initialize(); return collection.IndexOf(value); }
        public void Insert(int index, object value) { Initialize(); collection.Insert(index, value); }
        public bool IsFixedSize { get { Initialize(); return false; } }
        public bool IsReadOnly { get { Initialize(); return false; } }
        public void Remove(object value) { Initialize(); collection.Remove(value); }
        public void RemoveAt(int index) { Initialize(); collection.RemoveAt(index); }
        public object this[int index]
        {
            get { Initialize(); return collection[index]; }
            set { Initialize(); collection[index] = value; }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index) { Initialize(); foreach (var item in collection) array.SetValue(item, index++); }
        public int Count { get { Initialize(); return collection.Count; } }
        public bool IsSynchronized { get { Initialize(); return false; } }
        public object SyncRoot { get { Initialize(); return syncRoot; } }
        private object syncRoot = new object();

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator() { Initialize(); return collection.GetEnumerator(); }

        #endregion
    }
}
