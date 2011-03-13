using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{

#if DEBUG

    public class CollectionDebugView
    {
        public CollectionDebugView(IEnumerable collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        protected IEnumerable collection;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get { return collection == null ? null : ToGeneric(collection).Select(item => item as object).ToArray(); }
        }

        public static IEnumerable<object> ToGeneric(IEnumerable collection)
        {
            foreach (object item in collection) yield return item;
        }
    }

#endif

}
