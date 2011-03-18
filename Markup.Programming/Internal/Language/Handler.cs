using System.Windows;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public abstract class Handler : PrimitiveActiveComponent
    {
        internal bool TopLevelOperation { get; set; }

        public string Path { get; set; }

        private PathExpression pathExpression = new PathExpression();
        protected PathExpression PathExpression { get { return pathExpression; } }

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

        private IDictionary<string, object> closure;

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
            if (TopLevelOperation) Process(new Engine());
        }

        protected override object OnProcess(Engine engine)
        {
            SetContext(engine);
            OnActiveExecute(engine);
            closure = engine.GetClosure();
            return null;
        }

        protected override void OnExecute(Engine engine)
        {
            engine.Throw("active operation executed as statement");
        }

        protected abstract void OnActiveExecute(Engine engine);

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
            new Engine(sender, args).With(this, closure, engine => EventHandler(engine));
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

        protected override void ExecuteBody(Engine engine)
        {
            SetContext(engine);
            base.ExecuteBody(engine);
        }
    }
}
