using System.Collections;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public class Pair : ValueExpression
    {
        public object Key
        {
            get { return (object)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.Register("Key", typeof(object), typeof(Pair), null);

        public string KeyPath { get; set; }

        private CodeTree keyCodeTree = new CodeTree();
        protected CodeTree KeyCodeTree { get { return keyCodeTree; } }

        public string ValuePath { get { return Path; } set { Path = value; } }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(KeyProperty);
        }

        protected override object OnGet(Engine engine)
        {
            return new DictionaryEntry(engine.Evaluate(KeyProperty, KeyPath, KeyCodeTree), engine.Evaluate(ValueProperty, ValuePath, CodeTree));
        }
    }
}
