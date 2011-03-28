using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Markup.Programming.Core
{
    public class ObjectNode : ExpressionNode
    {
        private Type dynamicType;

        public ExpressionNode Type { get; set; }
        public IList<InitializerProperty> Properties { get; set; }
        protected override object OnGet(Engine engine)
        {
            var context = null as object;
            if (Type != null)
                context = TypeHelper.CreateInstance(Type.Get(engine) as Type);
            else
            {
                var pairs = Properties.Select(property => new NameValuePair(property.PropertyName, null)).ToArray();
                var dynamicObject = DynamicHelper.CreateObject(ref dynamicType, pairs) as IDynamicObject;
                foreach (var property in Properties)
                {
                    if (property.IsCollection) dynamicObject[property.PropertyName] = new ObservableCollection<object>();
                    if (property.IsDictionary) dynamicObject[property.PropertyName] = new Dictionary<object, object>();
                }
                context = dynamicObject;
            }
            foreach (var property in Properties)
            {
                if (property.IsCollection)
                {
                    var dictionary = PathHelper.GetProperty(engine, context, property.PropertyName) as IDictionary;
                    foreach (var value in property.Values)
                    {
                        var entry = (DictionaryEntry)value.Get(engine);
                        dictionary.Add(entry.Key, entry.Value);
                    }
                }
                else if (property.IsDictionary)
                {
                    var collection = PathHelper.GetProperty(engine, context, property.PropertyName) as IList;
                    foreach (var value in property.Values) collection.Add(value.Get(engine));
                }
                else
                    PathHelper.SetProperty(engine, context, property.PropertyName, property.Value.Get(engine));
            }
            return context;
        }
    }
}
