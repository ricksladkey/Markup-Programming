using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Data;
using Markup.Programming.Core;
using System.Collections.ObjectModel;

namespace Markup.Programming.Tests
{
    [TestClass]
    public class EvaluationTests
    {
        [TestMethod]
        public void NewGenericListTest()
        {
            Test.ExpressionTest(new List<string>(),
                new New
                {
                    Type = typeof(List<>),
                    TypeArguments = { new Val { Value = typeof(string) } },
                });
        }

        [TestMethod]
        public void IsTest()
        {
            Test.ExpressionTest(true, new Op { Value1 = 1, Operator = Operator.Is, Value2 = typeof(int) });
            Test.ExpressionTest(false, new Op { Value1 = 1.0, Operator = Operator.Is, Value2 = typeof(int) });
            Test.ExpressionTest(true, new Op { Value1 = new Dog(), Operator = Operator.Is, Value2 = typeof(Animal) });
            Test.ExpressionTest(false, new Op { Value1 = new Animal(), Operator = Operator.Is, Value2 = typeof(Dog) });
        }

        [TestMethod]
        public void ExpressionTests()
        {
            Test.ExpressionTest(3, new Op { Operator = Operator.Plus,
                Arguments = { new Val { Value = 1 }, new Val { Value = 2 } } });
        }

        [TestMethod]
        public void CallCosTest()
        {
            Test.StatementTest(0.0, 1.0,
                Test.Configure(new Set
                    {
                        PropertyName = "Object1",
                        Value = new Call
                        {
                            Type = typeof(Math),
                            StaticMethodName = "Cos",
                            Arguments =
                            {
                                Test.Configure(new Val(),
                                    value => BindingOperations.SetBinding(value, Val.ValueProperty, new Binding("Object1"))),
                            },
                        },
                    },
                    Test.TargetBinder));
        }

        [TestMethod]
        public void FormatTest()
        {
            // Call the static String.Format method with explicit
            // type arguments and test that params are working.
            Test.ExpressionTest("1, 2",
                new Call
                {
                    Type = typeof(string),
                    StaticMethodName = "Format",
                    TypeArguments =
                    {
                        new Val { Value = typeof(string) },
                        new Val { Value = typeof(object[]) },
                    },
                    Arguments =
                    {
                        new Val { Value = "{0}, {1}" },
                        new Val { Value = 1 },
                        new Val { Value = 2 },
                    }
                });
        }

        [TestMethod]
        public void IteratorTest()
        {
            // Create an collection with an iterator and check that
            // the collection has the correct type and contents 
            // and the deduced generic type is the minimum base class.
            var item1 = new Dog();
            var item2 = new Animal();
            Test.ExpressionTest(new ObservableCollection<Animal> { item1, item2 },
                new Iterator
                {
                    Body =
                    {
                        new Yield { Value = item1 },
                        new Yield { Value = item2 },
                    }
                });
        }

        [TestMethod]
        public void GetItemTest()
        {
            var array = new int[] { 1, 2, 3 };
            var value1 = new GetItem { Context = array, Index = 1 }.Evaluate(new Engine());
            Assert.AreEqual(2, value1);
            var list = new List<int> { 1, 2, 3 };
            var value2 = new GetItem { Context = list, Index = 1 }.Evaluate(new Engine());
            Assert.AreEqual(2, value2);
            var dictionary = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            var value3 = new GetItem { Context = dictionary, Index = "b" }.Evaluate(new Engine());
            Assert.AreEqual(2, value2);
        }

        [TestMethod]
        public void SetItemTest()
        {
            var array = new List<int> { 1, 2, 3 };
            new SetItem { Context = array, Index = 1, Value = 42 }.Evaluate(new Engine());
            Assert.AreEqual(42, array[1]);
            var list = new List<int> { 1, 2, 3 };
            new SetItem { Context = list, Index = 1, Value = 42 }.Evaluate(new Engine());
            Assert.AreEqual(42, list[1]);
            var dictionary = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            new SetItem { Context = dictionary, Index = "b", Value = 42 }.Evaluate(new Engine());
            Assert.AreEqual(42, dictionary["b"]);
        }
    }
}
