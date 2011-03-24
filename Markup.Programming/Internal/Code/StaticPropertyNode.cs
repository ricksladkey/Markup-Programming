using System;

namespace Markup.Programming.Core
{
    public class StaticPropertyNode : ExpressionNode
    {
        public TypeNode Type { get; set; }
        public string PropertyName { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            var type = Type.Evaluate(engine);
            return PathHelper.GetStaticProperty(engine, type as Type, PropertyName);
        }
        protected override void OnSet(Engine engine, object value)
        {
            var type = Type.Evaluate(engine);
            PathHelper.SetStaticProperty(engine, type as Type, PropertyName, value);
        }
    }
}
