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
            if (Type == null)
            {
                var pairs = Properties.Select(property => new NameValuePair(property.PropertyName, null)).ToArray();
                var dynamicObject = DynamicHelper.CreateObject(ref dynamicType, pairs) as IDynamicObject;
                foreach (var property in Properties)
                {
                    if (property.IsCollection)
                    {
                        var values = property.Values.Select(value => value.Get(engine)).ToArray();
                        dynamicObject[property.PropertyName] = TypeHelper.CreateCollection(values);
                    }
                    else if (property.IsDictionary)
                    {
                        var dictionary = new Dictionary<object, object>();
                        foreach (var value in property.Values)
                        {
                            var entry = (DictionaryEntry)value.Get(engine);
                            dictionary.Add(entry.Key, entry.Value);
                        }
                        dynamicObject[property.PropertyName] = dictionary;
                    }
                    else
                        dynamicObject[property.PropertyName] = property.Value.Get(engine);
                }
                return dynamicObject;
            }
            var context = TypeHelper.CreateInstance(Type.Get(engine) as Type);
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
