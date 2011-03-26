using System.Windows;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public abstract class Handler : PrimitiveActiveComponent
    {
        public static string AttachedKey = "@Attached";

        internal IDictionary<string, object> State { get; set; }

        public string Path { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

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
            if (State != null) new Engine().FrameFunc(this, State, OnProcess);
            var state = State;
        }

        protected override object OnProcess(Engine engine)
        {
            OnExecute(engine);
            return null;
        }

        protected override void OnExecute(Engine engine)
        {
            SetContext(engine);
            OnActiveExecute(engine);
        }

        protected abstract void OnActiveExecute(Engine engine);

        protected void SetContext(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath, ContextCodeTree);
        }

        private string registeredEventName;
        private static MethodInfo handlerMethodInfo = typeof(Handler).GetMethod("EventHandler");

        protected void RegisterHandler(Engine engine, string alternateEventName)
        {
            var context = AssociatedObject;
            registeredEventName = EventName ?? alternateEventName ?? AttachedKey;
            if (registeredEventName == AttachedKey)
            {
                EventHandler(this, null);
                return;
            }
            var eventInfo = context.GetType().GetEvent(registeredEventName);
            if (eventInfo == null) engine.Throw("no such event: " + registeredEventName);
            eventInfo.AddEventHandler(context,
                Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethodInfo));
        }

        public void EventHandler(object sender, object args)
        {
            new Engine(sender, args).FrameAction(this, State, EventHandler);
            if (State != null)
            {
                var state = State;
            }
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
            ExecuteBody(engine);
        }

        protected override void ExecuteBody(Engine engine)
        {
            SetContext(engine);
#if !INTERACTIVITY
            foreach (var statement in Body) statement.Execute(engine, State);
#else
            base.ExecuteBody(engine);
#endif
        }
    }
}
