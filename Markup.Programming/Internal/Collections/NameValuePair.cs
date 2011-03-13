using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A NameValuePair is like a KeyValuePair but
    /// more concise.  It is immutable.
    /// </summary>
    public struct NameValuePair
    {
        private string name;
        private object value;

        public NameValuePair(string name, object value)
        {
            this.name = name;
            this.value = value;
        }

        public string Name { get { return name; } }
        public object Value { get { return value; } }
    }

}
