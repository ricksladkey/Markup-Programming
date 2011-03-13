using System.Windows;
using System;
using System.Reflection;

namespace Markup.Programming.Core
{
    public abstract class Handler : PrimitiveActiveComponent, IComponent
    {
        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public string ContextPath { get; set; }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(Handler), null);

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(Handler), null);

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
        private static MethodInfo handlerMethodInfo = typeof(Handler).GetMethod("EventHandler");

        protected void RegisterHandler(Engine engine, string alternateEventName)
        {
            var context = AssociatedObject;
            registeredEventName = EventName ?? alternateEventName;
            var eventInfo = context.GetType().GetEvent(registeredEventName);
            if (eventInfo == null) ThrowHelper.Throw("no such event: " + registeredEventName);
            eventInfo.AddEventHandler(context,
                Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethodInfo));
        }

        public void EventHandler(object sender, object args)
        {
            new Engine(sender, args).With(this, engine => EventHandler(engine));
        }

        private void EventHandler(Engine engine)
        {
            engine.Trace(TraceFlags.Events, "Event: {0}, sender {1}", registeredEventName, engine.Sender);
            engine.SetContext(ContextProperty, ContextPath);
            OnEventHandler(engine);
            // XXX: Not right.
            if (engine.EventArgs is RoutedEventArgs)
                (engine.EventArgs as RoutedEventArgs).Handled = true;
        }

        protected virtual void OnEventHandler(Engine engine)
        {
        }
    }
}
