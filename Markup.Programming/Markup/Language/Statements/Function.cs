using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming
{
    /// <summary>
    /// A Function is a set of parameters and a collection of statements.
    /// The parameters associate names with positional arguments that are
    /// supplied when the function is called.  When the function is called
    /// the parameter names will be bound to the values of the supplied
    /// arguments for the duration of the execution of the function.
    /// To return a value use the Return statement.  A Function always
    /// produces a return value but to return null simply fall off
    /// the end of the function.
    /// </summary>
    public class Function : Block
    {
        public Function()
        {
            Parameters = new ParameterCollection();
        }

        public string FunctionName { get; set; }

        public ParameterCollection Parameters
        {
            get { return (ParameterCollection)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }

        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register("Parameters", typeof(ParameterCollection), typeof(Function), null);

        public bool HasParamsParameter
        {
            get { return Parameters.Count > 0 && Parameters[Parameters.Count - 1].Params; }
        }

        protected override void OnExecute(Engine engine)
        {
            engine.DefineFunction(FunctionName, this);
        }
    }
}
