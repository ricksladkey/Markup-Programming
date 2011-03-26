using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Markup.Programming.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;

namespace Markup.Programming.Tests.Tests
{
    [TestClass]
    public class CodeTests
    {
        /// <summary>
        /// THe PathEvaluator is sort of a hybrid between p:Get and p:Set
        /// which allows us to retrieve the entire variable state after
        /// evaluation.
        /// </summary>
        private class PathEvaluator : IComponent
        {
            public void Attach(DependencyObject dependencyObject) { throw new NotImplementedException(); }
            public void Detach() { throw new NotImplementedException(); }
            public DependencyObject AssociatedObject { get { return null; } }
            public object GetPath(IDictionary<string, object> variables, string path)
            {
                return new Engine().FrameFunc(this, variables, engine => GetPath(variables, path, engine));
            }
            public object SetPath(IDictionary<string, object> variables, string path, object value)
            {
                return new Engine().FrameFunc(this, variables, engine => SetPath(variables, path, value, engine));
            }
            private object GetPath(IDictionary<string, object> variables, string path, Engine engine)
            {
                PrintCodeTree(new CodeTree().Compile(engine, CodeType.GetExpression, path));

                var result = engine.GetPath(path, null);
                foreach (var name in variables.Keys) variables[name] = engine.GetVariable(name);
                return result;
            }
            private object SetPath(IDictionary<string, object> variables, string path, object value, Engine engine)
            {
                var result = engine.SetPath(path, null, value);
                foreach (var name in variables.Keys) variables[name] = engine.GetVariable(name);
                return result;
            }

            private void PrintCodeTree(CodeTree codeTree)
            {
                PrintNode(codeTree.Root);
                Print(";\n");
            }

            private int indent = 0;

            private void PrintNode(Node node)
            {
                bool nested = false;
                foreach (var property in node.GetType().GetProperties())
                {
                    var value = property.GetValue(node, null);
                    if (value is Node || value is IList && (value as IList).Count > 0 && (value as IList)[0] is Node)
                    {
                        nested = true;
                        break;
                    }
                }
                if (!nested)
                {
                    Print("\n", Spaces(indent), node.GetType().Name, " { ");
                    bool first = true;
                    foreach (var property in node.GetType().GetProperties())
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
                    foreach (var property in node.GetType().GetProperties())
                    {
                        Print("\n", Spaces(indent));
                        var value = property.GetValue(node, null);
                        Print(property.Name, " = ");
                        if (value is Node)
                        {
                            indent += 4;
                            PrintNode(value as Node);
                            Print(",");
                            indent -= 4;
                            continue;
                        }
                        if (value is IList && (value as IList).Count > 0 && (value as IList)[0] is Node)
                        {
                            indent += 4;
                            Print("\n", Spaces(indent), "{");
                            indent += 4;
                            foreach (var item in value as IList) PrintNode(item as Node);
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
            private void Print(params object[] args) { foreach (var arg in args) Debug.Write(arg); }
            private string Spaces(int n) { var spaces = ""; for (int i = 0; i < n; i++) spaces += " "; return spaces; }
        }

        private void PathTest(object expectedValue, IDictionary<string, object> variables, string path)
        {
            TestHelper.AreStructurallyEqual(expectedValue, new PathEvaluator().GetPath(variables, path));
        }

        private void BasicGetTest(object expectedValue, string path)
        {
            PathTest(expectedValue, GetBasicVariables(), path);
        }

        private void BasicSetTest(Action<BasicViewModel, IDictionary<string, object>> action, string path, object value)
        {
            var variables = GetBasicVariables();
            new PathEvaluator().SetPath(variables, path, value);
            action(variables[Engine.ContextKey] as BasicViewModel, variables);
        }

        private IDictionary<string, object> GetBasicVariables()
        {
            var viewModel = new BasicViewModel
            {
                String1 = "Test1",
                Object1 = new BasicViewModel { String1 = "Test2" },
            };
            var names = new NameDictionary
            {
                { Engine.ContextKey, viewModel },
                { "$variable1", "Value1" },
            };
            return names;
        }

        [TestMethod]
        public void BasicPathTests()
        {
            BasicGetTest("abc", "'abc'");
            BasicGetTest(true, "true");
            BasicGetTest(3, "1 + 2");
            BasicGetTest(0.25, "0.5/2");
            BasicGetTest(-3, "-3");
            BasicGetTest(true, "1 == 1");
            BasicGetTest(true, "!false");
            BasicGetTest(typeof(HorizontalAlignment), "[HorizontalAlignment]");
            BasicGetTest("Left", "[Enum].GetValues([HorizontalAlignment])[0].ToString()");
            BasicGetTest(false, "@Convert('fALSE', [Boolean])");
            BasicGetTest(true, "@Convert('True', [Boolean])");
            BasicGetTest(6, "@Op('Plus', 1, 2, 3)");
            BasicGetTest(new ArrayList(), "@Op('New', [ArrayList])");
            BasicGetTest(new ArrayList(), "[ArrayList]()");
            BasicGetTest(42, "true ? 42 : 21");
            BasicGetTest(typeof(int), "[Int32]");
            BasicGetTest(typeof(List<>), "[List<>]");
            BasicGetTest(typeof(List<int>), "[List<Int32>]");
            BasicGetTest(typeof(List<List<int>>), "[List<List<Int32>>]");
            BasicGetTest("123", "123 .ToString()");
            BasicGetTest(new Point { X = 1, Y = 2 }, "[Point] { X = 1,  Y =  2 }");
            BasicGetTest(new List<int> { 1, 2, 3 }, "[List<int>] { 1, 2, 3 }");
            BasicGetTest(3, "1, 3");
            BasicGetTest(3, "1 + /* 37 * 99 */ 2");
            BasicGetTest(3, "1 + /* /* 37 * 99 */ xyzzy */ 2");
            BasicGetTest(2, "[List<int>] { 1, 2, 3 }[1]");
            BasicGetTest(42, "@block: { return 42; }");
            BasicGetTest(true, "// comment; true");
            var foo = typeof(MouseButtonState);
            BasicGetTest(true, "[MouseButtonState].Pressed != [MouseButtonState].Released");
            BasicGetTest(7, "1 + 2 * 3");
            BasicGetTest(7, "2 * 3 + 1");
            BasicGetTest(2, "false ? 1 : false ? 1 : 2");
            BasicGetTest(null, "$i");
            BasicGetTest(1, "$i = 1");
            BasicGetTest(3, "$i = 1, $i += 2");

            BasicGetTest("Test1", "String1");
            BasicGetTest("Test2", "Object1.String1");
            BasicGetTest(true, "@VariableIsDefined('$variable1')");
            BasicGetTest("Value1", "$variable1");
            BasicGetTest(256.0, "[System.Math].Pow(2, 8)");
            BasicSetTest((vm, p) => Assert.AreEqual(p["$variable1"], 42), "$variable1", 42);
        }

        [TestMethod]
        public void PathTestSandbox()
        {
        }

        [TestMethod]
        public void ScriptTests()
        {
            TestHelper.ScriptTest(21, "var $x = 1 + 2; return 42 / 2;");
            TestHelper.ScriptTest(42, "var $x = true; if ($x) return 42; return 21;");
            TestHelper.ScriptTest(42, "var $x = true; if (!$x) return 21; else return 42;");
            TestHelper.ScriptTest("b", "var $x = 2; if ($x == 1) return 'a'; else if ($x == 2) return 'b'; else return 'c';");
            TestHelper.ScriptTest(5, "var $i = 0; while ($i < 5) $i = $i + 1; return $i;");
            TestHelper.ScriptTest(0, "{ 1; 2; } return 0;");
            TestHelper.ScriptTest(5, "var $i = 0; while ($i < 10) { $i = $i + 1; if ($i == 5) break; } return $i;");
            TestHelper.ScriptTest(6, "var $x = 0; foreach (var $item in [List<int>] { 1, 2, 3 }) $x = $x + $item; return $x;");
            TestHelper.ScriptTest(2, "var $total = 0; $total += 2; return $total;");
            TestHelper.ScriptTest(3, "var $i = 2; return $i + 1;");
            TestHelper.ScriptTest(6, "var $total = 0; for (var $i = 1; $i <= 3; $i++) $total += $i; return $total;");
            TestHelper.ScriptTest(new ObservableCollection<int> { 1, 2, 3 }, "return @iterator: { yield 1; yield 2; yield 3; }");
            TestHelper.ScriptTest(new List<int> { 1, 2, 3 }, "return @iterator[List<>]: { yield 1; yield 2; yield 3; }");
            TestHelper.ScriptTest(new List<int> { 1, 2, 3 }, "return @iterator[List<int>]: { yield 1; yield 2; yield 3; }");
            TestHelper.ScriptTest(null, "for (;;) break;");
            TestHelper.ScriptTest(3, "var $Add($a, $b) { return $a + $b; }; return $Add(1, 2);");
            TestHelper.ScriptTest(3, "var $point = [] { X = 1, Y = 2 }; return $point.X + $point.Y;");
        }

        [TestMethod]
        public void ScriptTestSandbox()
        {
        }
    }
}
