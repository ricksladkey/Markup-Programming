using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using Markup.Programming.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Markup.Programming.Tests.Tests
{
    [TestClass]
    public class PathTests
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
            public object Evaluate(IDictionary<string, object> variables, string path)
            {
                return new Engine().With(this, variables, engine => GetPath(variables, path, engine));
            }
            public object Evaluate(IDictionary<string, object> variables, string path, object value)
            {
                return new Engine().With(this, variables, engine => SetPath(variables, path, value, engine));
            }
            private object GetPath(IDictionary<string, object> variables, string path, Engine engine)
            {
                var result = engine.GetPath(path, null);
                foreach (var name in variables.Keys) variables[name] = engine.LookupVariable(name);
                return result;
            }
            private object SetPath(IDictionary<string, object> variables, string path, object value, Engine engine)
            {
                var result = engine.SetPath(path, null, value);
                foreach (var name in variables.Keys) variables[name] = engine.LookupVariable(name);
                return result;
            }
        }

        private void PathTest(object expectedValue, IDictionary<string, object> variables, string path)
        {
            TestHelper.AreStructurallyEqual(expectedValue, new PathEvaluator().Evaluate(variables, path));
        }

        private void BasicGetTest(object expectedValue, string path)
        {
            PathTest(expectedValue, GetBasicVariables(), path);
        }

        private void BasicSetTest(Action<BasicViewModel, IDictionary<string, object>> action, string path, object value)
        {
            var variables = GetBasicVariables();
            new PathEvaluator().Evaluate(variables, path, value);
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
            BasicGetTest(true, "@true");
            BasicGetTest(3, "1 + 2");
            BasicGetTest(0.25, "0.5/2");
            BasicGetTest(true, "1 == 1");
            BasicGetTest(true, "!@false");
            BasicGetTest("Left", "[Enum].GetValues([HorizontalAlignment])[0].ToString()");
            BasicGetTest(false, "@Convert('fALSE', [Boolean])");
            BasicGetTest(true, "@Convert('True', [Boolean])");
            BasicGetTest(6, "@Op('Plus', 1, 2, 3)");
            BasicGetTest(new ArrayList(), "@Op('New', [ArrayList])");
            BasicGetTest(new ArrayList(), "[ArrayList]()");
            BasicGetTest(42, "@true ? 42 : 21");
            BasicGetTest(typeof(int), "[Int32]");
            BasicGetTest(typeof(List<>), "[List<>]");
            BasicGetTest(typeof(List<int>), "[List<Int32>]");
            BasicGetTest(typeof(List<List<int>>), "[List<List<Int32>>]");
            BasicGetTest("123", "123 .ToString()");
            BasicGetTest(new Point { X = 1, Y = 2 }, "[Point] { X = 1,  Y =  2 }");
            BasicGetTest(new List<int> { 1, 2, 3 }, "[List<int>] { 1, 2, 3 }");
            BasicGetTest(3, "1, 3");
            BasicGetTest(42, "($theAnswer = 42), $theAnswer");
            BasicGetTest(3, "1 + /* 37 * 99 */ 2");
            BasicGetTest(3, "1 + /* /* 37 * 99 */ xyzzy */ 2");
            BasicGetTest(2, "[List<int>] { 1, 2, 3 }[1]");

            BasicGetTest("Test1", "String1");
            BasicGetTest("Test2", "Object1.String1");
            BasicGetTest(true, "@ParameterIsDefined('$variable1')");
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
            TestHelper.ScriptTest(21, "$x = 1 + 2; return 42 / 2;");
            TestHelper.ScriptTest(42, "$x = @true; if ($x) return 42; return 21;");
            TestHelper.ScriptTest(42, "$x = @true; if (!$x) return 21; else return 42;");
            TestHelper.ScriptTest("b", "$x = 2; if ($x == 1) return 'a'; else if ($x == 2) return 'b'; else return 'c';");
        }
    }
}
