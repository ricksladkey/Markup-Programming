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

        public string WhenPath { get; set; }

        public static readonly DependencyProperty WhenProperty =
            DependencyProperty.Register("When", typeof(object), typeof(ConditionalValueStatement), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(WhenProperty);
        }

        protected bool ShouldExecute(Engine engine)
        {
            var when = engine.Evaluate(WhenProperty, WhenPath);
            if (when == null) return true;
            return TypeHelper.ConvertToBool(when);
        }
    }
}
