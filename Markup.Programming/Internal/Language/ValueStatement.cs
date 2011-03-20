using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A ValueStatement is an UntypedValueStatement with
    /// a Type property.
    /// </summary>
    public abstract class ValueStatement : UntypedValueStatement
    {
        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(ValueStatement), null);

        public string TypePath { get; set; }

        private PathExpression typePathExpression = new PathExpression();
        protected PathExpression TypePathExpression { get { return typePathExpression; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty);
        }
    }
}
