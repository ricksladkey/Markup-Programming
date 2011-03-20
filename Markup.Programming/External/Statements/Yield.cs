using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Yield statement adds an item or items to the collection being
    /// collected by the nearest enclosing Iterator.  The item added is Value
    /// optionally converted to type Type, or if Merge is true, items are Value
    /// interpreted as a System.Collections.IList with each item optionally
    /// converted to type Type.
    /// </summary>
    public class Yield : ConditionalValueStatement
    {
        public bool Merge
        {
            get { return (bool)GetValue(MergeProperty); }
            set { SetValue(MergeProperty, value); }
        }

        public static readonly DependencyProperty MergeProperty =
            DependencyProperty.Register("Merge", typeof(bool), typeof(Yield), null);

        protected override void OnExecute(Engine engine)
        {
            if (!ShouldExecute(engine)) return;
            var type = engine.EvaluateType(TypeProperty, TypePath, TypePathExpression);
            var value = engine.Evaluate(ValueProperty, Path, PathExpression, type);
            if (Merge)
                foreach (var item in value as IList) engine.YieldValue(TypeHelper.Convert(item, type));
            else
                engine.YieldValue(value);
        }
    }
}
