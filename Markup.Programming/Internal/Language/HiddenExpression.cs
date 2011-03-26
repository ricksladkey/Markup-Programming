using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An HiddenExpression is an IHiddenExpression or simply a Statement
    /// that produces a value but doesn't show up in an expression context.
    /// In other words, in cannot belong to an ExpressionCollection but
    /// if assigned to a property of type object that is evaluated if will
    /// act like an ordinary expression.  Implementing classes must override
    /// OnGet.
    /// </summary>
    public abstract class HiddenExpression : Statement, IHiddenExpression
    {
        protected override void OnExecute(Engine engine)
        {
            // Nothing happens.
        }

        public object Get(Engine engine)
        {
            // Inline Process(engine);
            return engine.FrameFunc(this, OnProcess);
        }

        protected override object OnProcess(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath, ContextCodeTree);
            return OnGet(engine);
        }

        protected abstract object OnGet(Engine engine);
    }
}
