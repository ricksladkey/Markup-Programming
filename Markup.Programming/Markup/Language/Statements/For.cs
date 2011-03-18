using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The For statement sets VariableName of optional type Type
    /// to initial value Value, executes its body while the While
    /// value evaluates to true and then sets VariableName to the value
    /// Next at end of each iteration.  The Type property defaults
    /// to System.Int32.  The While property effectively defaults
    /// to true.  The Break statement can be used to break out of
    /// the loop.  VariableName goes out of scope after the
    /// statement.  If VariableName is omitted, no other actions
    /// will be taken except for repeatedly executing the loop.
    /// 
    /// There is also a simplified alternative for numerically increasing
    /// loops.  If While is not specified but UpperLimit is, then While
    /// interpreted as the VariableName being less than UpperLimit.
    /// If Next is not specified but Increment is, then Next is
    /// interpreted as VariableName plus the value of Increment.
    /// Note that Operator.LessThan and Operator.Add will be chosen
    /// based on Type.
    /// </summary>
    public class For : VariableBlock
    {
        public For()
        {
            Next = new StatementCollection();
        }

        public object While
        {
            get { return (object)GetValue(WhileProperty); }
            set { SetValue(WhileProperty, value); }
        }

        public string WhilePath { get; set; }

        private PathExpression whilePathExpression = new PathExpression();
        protected PathExpression WhilePathExpression { get { return whilePathExpression; } }

        public static readonly DependencyProperty WhileProperty =
            DependencyProperty.Register("While", typeof(object), typeof(For), null);

        public StatementCollection Next
        {
            get { return (StatementCollection)GetValue(NextProperty); }
            set { SetValue(NextProperty, value); }
        }

        public static readonly DependencyProperty NextProperty =
            DependencyProperty.Register("Next", typeof(StatementCollection), typeof(For), null);

        public object UpperLimit
        {
            get { return (object)GetValue(UpperLimitProperty); }
            set { SetValue(UpperLimitProperty, value); }
        }

        public string UpperLimitPath { get; set; }

        private PathExpression upperLimitPathExpression = new PathExpression();
        protected PathExpression UpperLimitPathExpression { get { return upperLimitPathExpression; } }

        public static readonly DependencyProperty UpperLimitProperty =
            DependencyProperty.Register("UpperLimit", typeof(object), typeof(For), null);

        public object Increment
        {
            get { return (object)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }

        public string IncrementPath { get; set; }

        private PathExpression incrementPathExpression = new PathExpression();
        protected PathExpression IncrementPathExpression { get { return incrementPathExpression; } }

        public static readonly DependencyProperty IncrementProperty =
            DependencyProperty.Register("Increment", typeof(object), typeof(For), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ValueProperty, WhileProperty, NextProperty, UpperLimitProperty, IncrementProperty);
        }

        protected override void OnExecute(Engine engine)
        {
            // Use the same name and type for the whole loop.
            var name = VariableName;
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (type == null && (UpperLimit != null || Increment != null)) type = typeof(int);

            // If no name is specified then forever.
            if (name == null)
            {
                engine.SetBreakFrame();
                while (!engine.ShouldInterrupt) Body.Execute(engine);
                return;
            }

            // Normal loop processing.
            SetLoopValue(name, engine.Evaluate(ValueProperty, Path, PathExpression, type), type, engine);
            engine.SetBreakFrame();
            while (true)
            {
                if (engine.ShouldInterrupt) break;
                if (While != null)
                {
                    if (!(bool)engine.Evaluate(WhileProperty, WhilePath, WhilePathExpression, typeof(bool))) break;
                }
                else if (UpperLimit != null)
                {
                    var limit = engine.Evaluate(UpperLimitProperty, UpperLimitPath, UpperLimitPathExpression, type);
                    if (!(bool)engine.Evaluate(Operator.LessThan, GetLoopValue(name, type, engine), limit)) break;
                }
                Body.Execute(engine);
                if (Next.Count != 0)
                    Next.Execute(engine);
                else if (Increment != null)
                {
                    var increment = engine.Evaluate(IncrementProperty, IncrementPath, IncrementPathExpression, type);
                    SetLoopValue(name, engine.Evaluate(Operator.Plus, GetLoopValue(name, type, engine), increment), type, engine);
                }
            }
        }

        private void SetLoopValue(string name, object value, Type type, Engine engine)
        {
            engine.DefineVariable(name, TypeHelper.Convert(value, type));
        }

        private object GetLoopValue(string name, Type type, Engine engine)
        {
            return TypeHelper.Convert(engine.LookupVariable(name), type);
        }
    }
}
