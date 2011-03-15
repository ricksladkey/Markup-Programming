using System;
using System.Windows;

namespace Markup.Programming.Core
{
    public interface ICaller
    {
        object Parameter { get; set; }
        string ParameterPath { get; set; }
        ExpressionCollection Arguments { get; set; }
        string FunctionName { get; set; }
        BuiltinFunction BuiltinFunction { get; set; }
        string StaticMethodName { get; set; }
        string MethodName { get; set; }
        string TypeName { get; set; }
        Type Type { get; set; }
        ExpressionCollection TypeArguments { get; set; }
        string PathBase { get; }
        DependencyProperty CallerTypeProperty { get; }
        DependencyProperty CallerParameterProperty { get; }
    }
}
