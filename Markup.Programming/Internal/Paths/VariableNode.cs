﻿namespace Markup.Programming.Core
{
    public class VariableNode : ExpressionNode
    {
        protected override object OnEvaluate(Engine engine, object value)
        {
            if (!IsSet) return engine.LookupVariable(Name);
            return engine.DefineVariableInParentScope(Name, value);
        }
    }
}
