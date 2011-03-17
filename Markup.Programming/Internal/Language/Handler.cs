using System.Windows;
using System;
using System.Reflection;

namespace Markup.Programming.Core
{
    public abstract class Handler : PrimitiveActiveComponent, IComponent
    {
        public string Path { get; set; }

        private PathExpression pathExpression = new PathExpression();
        protected PathExpression PathExpression { get { return pathExpression; } }

        public object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public string ContextPath { get; set; }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(Handler), null);

        private PathExpression contextPathExpression = new PathExpression();
        protected PathExpression ContextPathExpression { get { return contextPathExpression; } }

        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(Handler), null);

        public bool SetHandled
        {
            get { return (bool)GetValue(SetHandledProperty); }
            set { SetValue(SetHandledProperty, value); }
        }

        public static readonly DependencyProperty SetHandledProperty =
            DependencyProperty.Register("SetHandled", typeof(bool), typeof(Handler), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
        }

        protected void Attach(params DependencyProperty[] properties)
        {
            ExecutionHelper.Attach(this, properties);
        }

        protected override void ExecuteBody(Engine engine)
        {
            SetContext(engine);
            base.ExecuteBody(engine);
        }

        protected void SetContext(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath, ContextPathExpression);
        }

        private string registeredEventName;
        private static MethodInfo handlerMethodInfo = typeof(Handler).GetMethod("EventHandler");

        protected void RegisterHandler(Engine engine, string alternateEventName)
        {
            var context = AssociatedObject;
            registeredEventName = EventName ?? alternateEventName;
            var eventInfo = context.GetType().GetEvent(registeredEventName);
            if (eventInfo == null) engine.Throw("no such event: " + registeredEventName);
            eventInfo.AddEventHandler(context,
                Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethodInfo));
        }

        public void EventHandler(object sender, object args)
        {
            new Engine(sender, args).With(this, engine => EventHandler(engine));
        }

        private void EventHandler(Engine engine)
        {
            if (Configuration.IsInDesignMode) return;
            engine.Trace(TraceFlags.Events, "Event: {0}, sender {1}", registeredEventName, engine.Sender);
            SetContext(engine);
            OnEventHandler(engine);
#if !SILVERLIGHT
            if (!Configuration.Silverlight && SetHandled && engine.EventArgs is RoutedEventArgs)
                (engine.EventArgs as RoutedEventArgs).Handled = true;
#endif
        }

        protected virtual void OnEventHandler(Engine engine)
        {
        }
    }
}
