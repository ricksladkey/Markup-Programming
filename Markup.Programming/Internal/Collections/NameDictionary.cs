using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A NameDictionary implements IDictionary
    /// and supplies NameValuePair-base constructors.
    /// </summary>
    public class NameDictionary : SmallDictionary<string, object>
    {
        public NameDictionary(params NameValuePair[] pairs)
            : this(pairs as IEnumerable<NameValuePair>)
        {
        }

        public NameDictionary(IEnumerable<NameValuePair> pairs)
        {
            foreach (var pair in pairs) Add(pair.Name, pair.Value);
        }
    }
}
