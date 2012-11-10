using System;
using System.Collections;
using System.Windows.Data;
using System.Windows;
using Markup.Programming.Core;
using System.Collections.Generic;
using Xunit;

namespace Markup.Programming.Tests
{
    public static class TestHelper
    {
        public static T Configure<T>(T value, params Action<T>[] actions)
        {
            foreach (var action in actions) action(value);
            return value;
        }

        public static void AttachAndExecute(DependencyObject target, params Statement[] statements)
        {
            var handler = new AttachedHandler();
            foreach (var statement in statements) handler.Actions.Add(statement);
            Attached.GetOperations(target).Add(handler);
        }

        public static void TargetBinder(Set statement)
        {
            BindingOperations.SetBinding(statement, Set.ContextProperty, new Binding());
        }

        public static void AreStructurallyEqual(object expected, object actual)
        {
            if (expected is IList)
            {
                Assert.Equal(expected.GetType(), actual.GetType());
                var expectedList = expected as IList;
                var actualList = actual as IList;
                Assert.NotNull(actualList);
                Assert.Equal(expectedList.Count, actualList.Count);
                for (int i = 0; i < expectedList.Count; i++)
                    Assert.Equal(expectedList[i], actualList[i]);
                return;
            }
            Assert.Equal(expected, actual);
        }

        public static void StatementTest(object initialValue, object expectedValue, Statement statement)
        {
            var viewModel = new BasicViewModel { Object1 = initialValue };
            var window = new Window { DataContext = viewModel };
            Assert.Equal(initialValue, viewModel.Object1);
            AttachAndExecute(window, statement);
            TestHelper.AreStructurallyEqual(expectedValue, viewModel.Object1);
        }

        public static void ExpressionTest(object expectedValue, IExpression expression)
        {
            var viewModel = new BasicViewModel();
            var window = new Window { DataContext = viewModel };
            var statement = TestHelper.Configure(new Set
                {
                    PropertyName = "Object1",
                    Value = expression,
                },
                TestHelper.TargetBinder);
            AttachAndExecute(window, statement);
            TestHelper.AreStructurallyEqual(expectedValue, viewModel.Object1);
        }

        public static void ScriptTest(object expectedValue, string body)
        {
            ExpressionTest(expectedValue, new Script { Body = body });
        }
    }
}
