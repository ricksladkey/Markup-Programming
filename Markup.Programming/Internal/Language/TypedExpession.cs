﻿using System;
using System.Windows;

namespace Markup.Programming.Core
{
    public abstract class TypedExpession : ExpressionBase
    {
        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(TypedExpession), null);

        public string TypePath { get; set; }

        private CodeTree typeCodeTree = new CodeTree();
        protected CodeTree TypeCodeTree { get { return typeCodeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty);
        }
    }
}
