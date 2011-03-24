using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Markup.Programming.Core
{
    public class ObjectNode : ExpressionNode
    {
        private Type dynamicType;

        public IList<InitializerProperty> Properties { get; set; }
        public ExpressionNode Type { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            if (Type != null) return TypeHelper.CreateInstance(Type.Evaluate(engine) as Type);
            var pairs = Properties.Select(property => new NameValuePair(property.PropertyName, null)).ToArray();
            var target = DynamicHelper.CreateObject(ref dynamicType, pairs) as IDynamicObject;
            foreach (var property in Properties)
            {
                if (property.IsCollection) target[property.PropertyName] = new ObservableCollection<object>();
                if (property.IsDictionary) target[property.PropertyName] = new Dictionary<object, object>();
            }
            return target;
        }
    }
}
