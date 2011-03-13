using System.Windows;
using System;
using System.Reflection;

namespace Markup.Programming.Core
{
    public abstract class HandlerBase : PrimitiveActiveComponent, IComponent
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

        private string registeredEventName;
        private static MethodInfo handlerMethodInfo = typeof(HandlerBase).GetMethod("Handler");

        protected void RegisterHandler(Engine engine, string alternateEventName)
        {
            var context = AssociatedObject;
            registeredEventName = EventName ?? alternateEventName;
            var eventInfo = context.GetType().GetEvent(registeredEventName);
            if (eventInfo == null) ThrowHelper.Throw("no such event: " + registeredEventName);
            eventInfo.AddEventHandler(context,
                Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethodInfo));
        }

        public void Handler(object sender, object args)
        {
            new Engine(sender, args).With(this, engine => Handler(engine));
        }

        private void Handler(Engine engine)
        {
            engine.Trace(TraceFlags.Events, "Event: {0}, sender {1}", registeredEventName, engine.Sender);
            engine.SetContext(ContextProperty, ContextPath);
            OnHandler(engine);
            // XXX: Not right.
            if (engine.EventArgs is RoutedEventArgs)
                (engine.EventArgs as RoutedEventArgs).Handled = true;
        }

        protected virtual void OnHandler(Engine engine)
        {
        }
    }
}
