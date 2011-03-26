using System.Collections;
using System.Diagnostics;
using System.Linq;
using Markup.Programming.Core;

namespace Markup.Programming.Tests
{
    public static class CodeTreeHelper
    {
        public static void Print(CodeTree codeTree)
        {
            PrintNode(codeTree.Root, 0);
            Print(";\n");
        }

        private static void PrintNode(Node node, int indent)
        {
            var properties = node.GetType().GetProperties().OrderBy(property => property.Name);
            bool nested = false;
            foreach (var property in properties)
            {
                var value = property.GetValue(node, null);
                if (IsNode(value) || IsNodeCollection(value))
                {
                    nested = true;
                    break;
                }
            }
            if (!nested)
            {
                Print("\n", Spaces(indent), node.GetType().Name, " { ");
                bool first = true;
                foreach (var property in properties)
                {
                    if (first)
                        first = false;
                    else
                        Print(", ");
                    Print(property.Name, " = ", property.GetValue(node, null));
                }
                Print(" }");
            }
            else
            {
                Print("\n", Spaces(indent), node.GetType().Name, "\n", Spaces(indent), "{");
                indent += 4;
                foreach (var property in properties)
                {
                    Print("\n", Spaces(indent));
                    var value = property.GetValue(node, null);
                    Print(property.Name, " = ");
                    if (IsNode(value))
                    {
                        indent += 4;
                        PrintNode(value as Node, indent);
                        Print(",");
                        indent -= 4;
                        continue;
                    }
                    if (IsNodeCollection(value))
                    {
                        indent += 4;
                        Print("\n", Spaces(indent), "{");
                        indent += 4;
                        foreach (var item in value as IList) PrintNode(item as Node, indent);
                        Print(",\n");
                        indent -= 4;
                        Print("\n", Spaces(indent), "},");
                        indent -= 4;
                        continue;
                    }
                    Print(value, ",");
                }
                indent -= 4;
                Print("\n", Spaces(indent), "}");
            }
        }

        private static bool IsNode(object value) { return value is Node; }
        private static bool IsNodeCollection(object value)
        {
            return value is IList && (value as IList).Count > 0 && (value as IList)[0] is Node;
        }
        private static void Print(params object[] args) { foreach (var arg in args) Debug.Write(arg); }
        private static string Spaces(int n) { var spaces = ""; for (int i = 0; i < n; i++) spaces += " "; return spaces; }
    }
}
