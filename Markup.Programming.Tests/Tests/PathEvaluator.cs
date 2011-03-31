using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming.Tests.Tests
{
    /// <summary>
    /// The PathEvaluator is sort of a hybrid between p:Get and p:Set
    /// which allows us to retrieve the entire variable state after
    /// evaluation.
    /// </summary>
    public class PathEvaluator : IComponent
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
            CodeTreeHelper.Print(new CodeTree().Compile(engine, CodeType.Get, path));
            var result = engine.GetPath(path, null);
            foreach (var name in variables.Keys) variables[name] = engine.GetVariable(name);
            return result;
        }
        private object SetPath(IDictionary<string, object> variables, string path, object value, Engine engine)
        {
            CodeTreeHelper.Print(new CodeTree().Compile(engine, CodeType.Set, path));
            var result = engine.SetPath(path, null, value);
            foreach (var name in variables.Keys) variables[name] = engine.GetVariable(name);
            return result;
        }
    }
}
