using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public class Case : ValueStatement, IHiddenExpression
    {
        protected override void OnExecute(Engine engine)
        {
            engine.Throw("Case only supported in Switch");
        }

        public object Get(Engine engine)
        {
            return engine.FrameFunc(this, EvaluateValue);
        }

        private object EvaluateValue(Engine engine)
        {
            var type = engine.GetType(TypeProperty, TypePath, TypeCodeTree);
            return engine.Get(ValueProperty, Path, CodeTree, type);
        }
    }
}
