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
    public class Function : StatementBlock
    {
        public Function()
        {
            Parameters = new ParameterCollection();
        }

        public string Func { get; set; }

        public string FunctionName { get; set; }

        public ParameterCollection Parameters { get; set; }

        public bool HasParamsParameter
        {
            get { return Parameters.Count > 0 && Parameters[Parameters.Count - 1].Params; }
        }

        protected override void OnExecute(Engine engine)
        {
            if (Func != null)
            {
                var func = Func;
                var open = func.IndexOf('(');
                var close = func.LastIndexOf(')');
                if (open == -1 || close == -1) engine.Throw("missing parentheses: " + func);
                FunctionName = func.Substring(0, open);
                var fields = func.Substring(open + 1, close - (open + 1)).Split(',');
                Parameters.AddRange(fields.Select(field => ParseParameter(engine, field.Trim())));
            }
            engine.DefineFunction("$" + FunctionName, this);
        }

        private Parameter ParseParameter(Engine engine, string parameter)
        {
            if (!parameter.Contains(' ')) return new Parameter { ParameterName = parameter};
            var fields = parameter.Split(' ');
            if (fields.Length != 2 || fields[0] != "params") engine.Throw("invalid params: " + parameter);
            return new Parameter { ParameterName = fields[1], Params = true };
        }
    }
}
