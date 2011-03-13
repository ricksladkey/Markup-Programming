using System;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A NameTypePair is like a KeyValuePair but
    /// more concise.  It is immutable.
    /// </summary>
    public struct NameTypePair
    {
        private string name;
        private Type type;

        public NameTypePair(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }

        public string Name { get { return name; } }
        public Type Type { get { return type; } }
    }

}
