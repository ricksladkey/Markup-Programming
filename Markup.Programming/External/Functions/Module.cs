using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows.Markup;
using System.Windows;
using System.ComponentModel;

namespace Markup.Programming
{
    /// <summary>
    /// A Module is a collection of functions.  It has no effect
    /// unless used with the Import statement.
    /// </summary>
    [ContentProperty("Functions")]
    public class Module : ResourceObjectBase
    {
        public Module()
        {
            Functions = new FunctionCollection();
        }

        public FunctionCollection Functions
        {
            get { return (FunctionCollection)GetValue(FunctionsProperty); }
            set { SetValue(FunctionsProperty, value); }
        }

        public static readonly DependencyProperty FunctionsProperty =
            DependencyProperty.Register("Functions", typeof(FunctionCollection), typeof(Module), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(FunctionsProperty);
        }

        public void Import(Engine engine)
        {
            TryToAttach();
            Functions.Execute(engine);
        }
    }
}
