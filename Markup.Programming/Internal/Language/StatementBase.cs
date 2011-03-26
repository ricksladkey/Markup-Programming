using System;
using System.Collections.Generic;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A StatementBase is a PassiveComponent that is an IStatement.  It is
    /// the base class that all Markup.Programming language primitive
    /// derive from.  As a base class it also includes utility methods
    /// associated with attaching and evalution that are applicable
    /// to all executable objects such as statements and expressions.
    /// </summary>
    public abstract class StatementBase : PrimitivePassiveComponent, IStatement
    {
        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(StatementBase), null);

        public string ContextPath { get; set; }

        private CodeTree contextCodeTree = new CodeTree();
        protected CodeTree ContextCodeTree { get { return contextCodeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
        }

        protected void Attach(params DependencyProperty[] properties)
        {
            ExecutionHelper.Attach(this, properties);
        }

        public void Execute(Engine engine)
        {
            // Inline Process(engine);
            engine.FrameFunc(this, OnProcess);
        }

        public void Execute(Engine engine, IDictionary<string, object> state)
        {
            engine.FrameFunc(this, state, OnProcess);
        }

        public object Process(Engine engine)
        {
            return engine.FrameFunc(this, OnProcess);
        }

        protected abstract object OnProcess(Engine engine);
    }
}
