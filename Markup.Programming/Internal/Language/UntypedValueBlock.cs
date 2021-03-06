﻿using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An UntypedValueBlock is a Block that also has Value and
    /// ValueParam properties but not Type.
    /// </summary>
    public abstract class UntypedValueBlock : StatementBlock
    {
        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(UntypedValueBlock), null);

        public string Path { get; set; }

        private CodeTree codeTree = new CodeTree();
        protected CodeTree CodeTree { get { return codeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty);
        }
    }
}
