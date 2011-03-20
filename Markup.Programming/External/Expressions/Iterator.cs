using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;
using System.Collections.ObjectModel;

namespace Markup.Programming
{
    /// <summary>
    /// The Iterator expression simply executes its body and
    /// returns a collection.  To add items to the collection
    /// that is returned, use the Yield statement.  To finish
    /// yielding items early use the Break statement.  The collection
    /// type Type defaults to ObservableCollection and TypeArgument
    /// defaults to a type deduced from the collection's contents.
    /// </summary>
    public class Iterator : BlockExpression
    {
        public object Type
        {
            get { return (object)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(object), typeof(Iterator), null);

        public string TypePath { get; set; }

        private PathExpression typePathExpression = new PathExpression();
        protected PathExpression TypePathExpression { get { return typePathExpression; } }

        public static readonly DependencyProperty TypeArgumentProperty =
            DependencyProperty.Register("TypeArgument", typeof(object), typeof(Iterator), null);

        public string TypeArgumentPath { get; set; }

        private PathExpression typeArgumentPathExpression = new PathExpression();
        protected PathExpression TypeArgumentPathExpression { get { return typeArgumentPathExpression; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(TypeProperty, TypeArgumentProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            engine.SetYieldFrame();
            Body.Execute(engine);
            var type = engine.EvaluateType(TypeProperty, TypePath, TypePathExpression);
            var typeArgument = engine.EvaluateType(TypeArgumentProperty, TypeArgumentPath, TypeArgumentPathExpression);
            return TypeHelper.CreateCollection(engine.GetYieldedValues(), type, typeArgument);
        }
    }
}
