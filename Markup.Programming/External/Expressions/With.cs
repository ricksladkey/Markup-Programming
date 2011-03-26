using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Expression")]
    public class With : ExpressionBase
    {
        public string Var { get; set; }

        public object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(With), null);

        public ExpressionBase Expression
        {
            get { return (ExpressionBase)GetValue(ExpressionProperty); }
            set { SetValue(ExpressionProperty, value); }
        }

        public static readonly DependencyProperty ExpressionProperty =
            DependencyProperty.Register("Expression", typeof(ExpressionBase), typeof(With), null);

        protected override void OnAttached()
        {
            Attach(ValueProperty, ExpressionProperty);
        }

        protected override object OnGet(Engine engine)
        {
            engine.DefineVariable(Var, engine.Get(ValueProperty, Path, CodeTree));
            return engine.Get(ExpressionProperty);
        }
    }
}
