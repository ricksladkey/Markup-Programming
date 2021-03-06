﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Windows;

namespace Markup.Programming.Core
{
    [ContentProperty("Script")]
    public abstract class InterfaceComponent : ResourceComponent, IInteropHost
    {
        public InterfaceComponent()
        {
            Functions = new FunctionCollection();
        }

        public string Script { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

        public FunctionCollection Functions
        {
            get { return (FunctionCollection)GetValue(FunctionsProperty); }
            set { SetValue(FunctionsProperty, value); }
        }

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.Register("Functions", typeof(FunctionCollection), typeof(InterfaceComponent), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(FunctionsProperty);
        }

        public object Callback(object child, string function, object[] args, Engine engine)
        {
            return engine.FrameFunc(this, e => CallFunction(child, function, args, engine));
        }

        private object CallFunction(object child, string function, object[] args, Engine engine)
        {
            engine.SetContext(null);
            if (Script != null) engine.ExecuteScript(Script, CodeTree);
            Functions.Execute(engine);
            return engine.CallFunction(function, args);
        }
    }
}
