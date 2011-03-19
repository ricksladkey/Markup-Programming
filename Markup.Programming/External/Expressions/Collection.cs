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
        public Type TypeArgument
        {
            get { return (Type)GetValue(TypeArgumentProperty); }
            set { SetValue(TypeArgumentProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentProperty =
            DependencyProperty.Register("TypeArgument", typeof(Type), typeof(Collection), null);

        public string TypeArgumentName { get; set; }

        protected override object OnEvaluate(Engine engine)
        {
            var items = Arguments.Evaluate(engine);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var typeArgument = engine.EvaluateType(TypeArgumentProperty, TypeArgumentName);
            return TypeHelper.CreateCollection(items, type, TypeArgument);
        }
    }
}
