﻿namespace Markup.Programming.Core
{
    public class ItemNode : PathNode
    {
        public PathNode Index { get; set; }
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return PathHelper.GetItem(engine, Context.Evaluate(engine, value), Index.Evaluate(engine, value));
            return PathHelper.SetItem(engine, Context.Evaluate(engine, value), Index.Evaluate(engine, value), value);
        }
    }
}
