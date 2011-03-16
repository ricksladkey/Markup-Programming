using System;
using System.Windows;

namespace Markup.Programming.Core
{
    public abstract class TypedExpession : ExpressionBase
    {
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(TypedExpession), null);

        public string TypeName { get; set; }
    }
}
