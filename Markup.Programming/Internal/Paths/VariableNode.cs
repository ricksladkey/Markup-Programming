﻿namespace Markup.Programming.Core
{
    public class VariableNode : PathNode
    {
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return engine.LookupVariable(Name);
            return engine.DefineVariableInParentScope(Name, value);
        }
    }
}
