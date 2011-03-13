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
        public Type Type
        {
            get { return (Type)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(Type), typeof(Iterator), null);

        public string TypeName
        {
            get { return (string)GetValue(TypeNameProperty); }
            set { SetValue(TypeNameProperty, value); }
        }

        public static readonly DependencyProperty TypeNameProperty =
            DependencyProperty.Register("TypeName", typeof(string), typeof(Iterator), null);


        public Type TypeArgument
        {
            get { return (Type)GetValue(TypeArgumentProperty); }
            set { SetValue(TypeArgumentProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentProperty =
            DependencyProperty.Register("TypeArgument", typeof(Type), typeof(Iterator), null);

        public string TypeArgumentName
        {
            get { return (string)GetValue(TypeArgumentNameProperty); }
            set { SetValue(TypeArgumentNameProperty, value); }
        }

        public static readonly DependencyProperty TypeArgumentNameProperty =
            DependencyProperty.Register("TypeArgumentName", typeof(string), typeof(Iterator), null);

        protected override object OnEvaluate(Engine engine)
        {
            engine.SetYieldFrame();
            Body.Execute(engine);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var typeArgument = engine.EvaluateType(TypeArgumentProperty, TypeArgumentName);
            return TypeHelper.CreateCollection(engine.GetYieldedValues(), type, typeArgument);
        }
    }
}
