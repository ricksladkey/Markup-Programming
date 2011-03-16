using System;
using System.Collections.Generic;
using System.Windows;
using Markup.Programming.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Markup.Programming.Tests.Tests
{
    [TestClass]
    public class PathTests
    {
        private class PathEvaluator : IComponent
        {
            public void Attach(DependencyObject dependencyObject) { throw new NotImplementedException(); }
            public void Detach() { throw new NotImplementedException(); }
            public DependencyObject AssociatedObject { get { return null; } }
            public object Evaluate(IDictionary<string, object> parameters, string path)
            {
                return new Engine().With(this, parameters, engine => GetPath(parameters, path, engine));
            }
            public object Evaluate(IDictionary<string, object> parameters, string path, object value)
            {
                return new Engine().With(this, parameters, engine => SetPath(parameters, path, value, engine));
            }
            private object GetPath(IDictionary<string, object> parameters, string path, Engine engine)
            {
                var result = engine.GetPath(path, null);
                foreach (var name in parameters.Keys) parameters[name] = engine.LookupParameter(name);
                return result;
            }
            private object SetPath(IDictionary<string, object> parameters, string path, object value, Engine engine)
            {
                var result = engine.SetPath(path, null, value);
                foreach (var name in parameters.Keys) parameters[name] = engine.LookupParameter(name);
                return result;
            }
        }

        private void PathTest(object expectedValue, IDictionary<string, object> parameters, string path)
        {
            TestHelper.AreStructurallyEqual(expectedValue, new PathEvaluator().Evaluate(parameters, path));
        }

        private void BasicGetTest(object expectedValue, string path)
        {
            PathTest(expectedValue, GetBasicParameters(), path);
        }

        private void BasicSetTest(Action<BasicViewModel, IDictionary<string, object>> action, string path, object value)
        {
            var parameters = GetBasicParameters();
            new PathEvaluator().Evaluate(parameters, path, value);
            action(parameters[Engine.ContextParameter] as BasicViewModel, parameters);
        }

        private IDictionary<string, object> GetBasicParameters()
        {
            var viewModel = new BasicViewModel
            {
                String1 = "Test1",
                Object1 = new BasicViewModel { String1 = "Test2" },
            };
            var names = new NameDictionary
            {
                { Engine.ContextParameter, viewModel },
                { "parameter1", "Value1" },
            };
            return names;
        }

        [TestMethod]
        public void BasicPathTests()
        {
            BasicGetTest("abc", "'abc'");
            BasicGetTest(true, "@true");
            BasicGetTest(3, "1 + 2");
            BasicGetTest(true, "1 == 1");
            BasicGetTest("Test1", "String1");
            BasicGetTest("Test2", "Object1.String1");
            BasicGetTest("Value1", "$parameter1");
            BasicGetTest(256.0, "[System.Math].Pow(2, 8)");
            BasicSetTest((vm, p) => Assert.AreEqual(p["parameter1"], 42), "$parameter1", 42);
        }
    }
}
