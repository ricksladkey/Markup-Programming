using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    public class Switch : ValueBlock
    {
        public Switch()
        {
            Default = new StatementCollection();
        }

        public StatementCollection Default
        {
            get { return (StatementCollection)GetValue(DefaultProperty); }
            set { SetValue(DefaultProperty, value); }
        }

        public static readonly DependencyProperty DefaultProperty =
            DependencyProperty.Register("Default", typeof(StatementCollection), typeof(Switch), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(DefaultProperty);
        }

        protected override void OnExecute(Engine engine)
        {
            var type = engine.EvaluateType(TypeProperty, TypeName);
            var value = engine.Evaluate(ValueProperty, Path, PathExpression, type);
            bool foundMatch = false;
            bool executedStatement = false;
            foreach (var statement in Body)
            {
                if (statement is Case)
                {
                    // Fell through to a new case group.
                    if (executedStatement) return;
                    var caseStatement = statement as Case;
                    if (!foundMatch)
                    {
                        var testValue = caseStatement.Evaluate(engine);
                        if (engine.ShouldInterrupt) return;
                        foundMatch = (bool)engine.Evaluate(Operator.Equals, value, testValue);
                    }
                }
                else
                {
                    // Found a match
                    if (foundMatch)
                    {
                        statement.Execute(engine);
                        executedStatement = true;
                        if (engine.ShouldInterrupt) return;
                    }
                }
            }
            if (!executedStatement) Default.Execute(engine);
        }
    }
}
