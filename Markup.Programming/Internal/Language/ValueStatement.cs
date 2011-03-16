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
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(ValueStatement), null);

        public string TypeName { get; set; }
    }
}
