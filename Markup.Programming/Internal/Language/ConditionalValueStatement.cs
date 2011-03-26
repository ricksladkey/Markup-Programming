using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{
    public abstract class ConditionalValueStatement : ValueStatement
    {
        public object When
        {
            get { return (object)GetValue(WhenProperty); }
            set { SetValue(WhenProperty, value); }
        }

        public static readonly DependencyProperty WhenProperty =
            DependencyProperty.Register("When", typeof(object), typeof(ConditionalValueStatement), null);

        public string WhenPath { get; set; }

        private CodeTree whenCodeTree = new CodeTree();
        protected CodeTree WhenCodeTree { get { return whenCodeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(WhenProperty);
        }

        protected bool ShouldExecute(Engine engine)
        {
            var when = engine.Get(WhenProperty, WhenPath, WhenCodeTree);
            if (when == null) return true;
            return TypeHelper.ConvertToBool(when);
        }
    }
}
