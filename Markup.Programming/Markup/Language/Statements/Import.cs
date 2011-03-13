using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    /// <summary>
    /// The Import statement imports a modules functions into the current scope.
    /// </summary>
    public class Import : Statement
    {
        public Module Module
        {
            get { return (Module)GetValue(ModuleProperty); }
            set { SetValue(ModuleProperty, value); }
        }

        public static readonly DependencyProperty ModuleProperty =
            DependencyProperty.Register("Module", typeof(Module), typeof(Import), null);

        protected override void OnExecute(Engine engine)
        {
            Module.Import(engine);
        }
    }
}
