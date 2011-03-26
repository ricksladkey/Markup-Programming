using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A Collection is an expression that evaluates to a collection
    /// and whose body evaluates to its items.  The collection type Type
    /// defaults to ObservableCollection and TypeArgument defaults to a
    /// type deduced from the collection's contents.
    /// </summary>
    public class Collection : ArgumentsExpressionWithType
    {
        public object TypeArgument
        {
            get { return (object)GetValue(TypeArgumentProperty); }
            set { SetValue(TypeArgumentProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentProperty =
            DependencyProperty.Register("TypeArgument", typeof(object), typeof(Collection), null);

        public string TypeArgumentPath { get; set; }

        private CodeTree typeArgumentCodeTree = new CodeTree();
        protected CodeTree TypeArgumentCodeTree { get { return typeArgumentCodeTree; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty, TypeArgumentProperty);
        }

        protected override object OnGet(Engine engine)
        {
            var items = Arguments.Evaluate(engine);
            var type = engine.EvaluateType(TypeProperty, TypePath, TypeCodeTree);
            var typeArgument = engine.EvaluateType(TypeArgumentProperty, TypeArgumentPath, TypeArgumentCodeTree);
            return TypeHelper.CreateCollection(items, type, typeArgument);
        }
    }
}
