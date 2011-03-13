using System.Windows;
using System;

namespace Markup.Programming.Core
{
    public abstract class HandlerBase : ActiveComponent, IComponent
    {
        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public string ContextPath { get; set; }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(HandlerBase), null);

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(HandlerBase), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
        }

        protected void Attach(params DependencyProperty[] properties)
        {
            ExecutionHelper.Attach(this, properties);
        }

        protected void SetContext(Engine engine)
        {
            if (Context != null || ContextPath != null || PathHelper.HasBinding(this, ContextProperty))
            {
                var context = engine.Evaluate(ContextProperty, ContextPath);
                engine.Trace(TraceFlags.Parameter, "Setting context = {0}", context);
                engine.DefineParameter(Engine.ContextParameter, context);
            }
        }

        protected void RegisterHandler(Engine engine, string alternateEventName)
        {
            var context = AssociatedObject;
            var eventName = EventName ?? alternateEventName;
            var eventInfo = context.GetType().GetEvent(eventName);
            var methodInfo = this.GetType().GetMethod("Handler");
            eventInfo.AddEventHandler(context,
                Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo));
        }

        public void Handler(object sender, object args)
        {
            new Engine(sender, args).With(this, engine => OnHandler(engine));
        }

        protected virtual void OnHandler(Engine engine)
        {
        }
    }
}
