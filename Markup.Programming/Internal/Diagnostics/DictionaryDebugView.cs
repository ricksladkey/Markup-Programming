using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;

namespace Markup.Programming.Core
{

#if DEBUG

    public class DictionaryDebugView
    {
        [DebuggerDisplay("{value}", Name = "{key}")]
        public struct KeyValuePair
        {
            public KeyValuePair(object key, object value) { this.key = key; this.value = value; }
            public object key;
            public object value;
        }

        protected DictionaryDebugView() { }

        private IDictionary dictionary;
        public DictionaryDebugView(IDictionary dictionary) { this.dictionary = dictionary; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public virtual KeyValuePair[] Pairs
        {
            get
            {
                if (dictionary == null) return null;
                var keys = CollectionDebugView.ToGeneric(dictionary.Keys);
                return keys.Select(key => new KeyValuePair(key, dictionary[key])).ToArray();
            }
        }
    }

    public class PropertyInfoDebugView : DictionaryDebugView
    {
        private IProvideProperties component;
        public PropertyInfoDebugView(IProvideProperties component) { this.component = component; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public override KeyValuePair[] Pairs
        {
            get
            {
                if (component == null) return null;
                return component.PropertyInfo.Select(property => new KeyValuePair(property.Name, component[property.Name])).ToArray();
            }
        }
    }

    public class DictionaryEntryDebugView : DictionaryDebugView
    {
        private IEnumerable collection;
        public DictionaryEntryDebugView(IEnumerable collection) { this.collection = collection; }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public override KeyValuePair[] Pairs
        {
            get
            {
                if (collection == null) return null;
                var entries = CollectionDebugView.ToGeneric(collection).Cast<DictionaryEntry>();
                return entries.Select(entry => new KeyValuePair(entry.Key, entry.Value)).ToArray();
            }
        }
    }

#endif

}
