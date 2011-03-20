using System.Collections;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public class KeyValuePair : ValueExpression
    {
        public object Key
        {
            get { return (object)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(object), typeof(KeyValuePair), null);

        public string KeyPath { get; set; }

        private PathExpression keyPathExpression = new PathExpression();
        protected PathExpression KeyPathExpression { get { return keyPathExpression; } }

        public string ValuePath { get { return Path; } set { Path = value; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(KeyProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            return new DictionaryEntry(engine.Evaluate(KeyProperty, KeyPath, KeyPathExpression), engine.Evaluate(ValueProperty, ValuePath, PathExpression));
        }
    }
}
