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
            TestHelper.ExpressionTest(new List<string>(),
                new New
                {
                    Type = typeof(List<>),
                    TypeArguments = { new Get { Source= typeof(string) } },
                });
        }

        [TestMethod]
        public void IsTest()
        {
            TestHelper.ExpressionTest(true, new Operator { Value1 = 1, Op = Op.Is, Value2 = typeof(int) });
            TestHelper.ExpressionTest(false, new Operator { Value1 = 1.0, Op = Op.Is, Value2 = typeof(int) });
            TestHelper.ExpressionTest(true, new Operator { Value1 = new Dog(), Op = Op.Is, Value2 = typeof(Animal) });
            TestHelper.ExpressionTest(false, new Operator { Value1 = new Animal(), Op = Op.Is, Value2 = typeof(Dog) });
        }

        [TestMethod]
        public void ExpressionTests()
        {
            TestHelper.ExpressionTest(3, new Operator { Op = Op.Plus,
                Arguments = { new Expr { Value = 1 }, new Expr { Value = 2 } } });
        }

        [TestMethod]
        public void CallCosTest()
        {
            TestHelper.StatementTest(0.0, 1.0,
                TestHelper.Configure(new Set
                    {
                        PropertyName = "Object1",
                        Value = new Call
                        {
                            Type = typeof(Math),
                            StaticMethodName = "Cos",
                            Arguments =
                            {
                                // <p:Expr Value="{Binding Object1}"/>
                                TestHelper.Configure(new Expr(),
                                    value => BindingOperations.SetBinding(value, Expr.ValueProperty, new Binding("Object1"))),
                            },
                        },
                    },
                    TestHelper.TargetBinder));
        }

        [TestMethod]
        public void FormatTest()
        {
            // Call the static String.Format method with explicit
            // type arguments and test that params are working.
            TestHelper.ExpressionTest("1, 2",
                new Call
                {
                    Type = typeof(string),
                    StaticMethodName = "Format",
                    TypeArguments =
                    {
                        new Expr { Value = typeof(string) },
                        new Expr { Value = typeof(object[]) },
                    },
                    Arguments =
                    {
                        new Expr { Value = "{0}, {1}" },
                        new Expr { Value = 1 },
                        new Expr { Value = 2 },
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
            TestHelper.ExpressionTest(new ObservableCollection<Animal> { item1, item2 },
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
            var value1 = new GetItem { Context = array, Index = 1 }.Get(new Engine());
            Assert.AreEqual(2, value1);
            var list = new List<int> { 1, 2, 3 };
            var value2 = new GetItem { Context = list, Index = 1 }.Get(new Engine());
            Assert.AreEqual(2, value2);
            var dictionary = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            var value3 = new GetItem { Context = dictionary, Index = "b" }.Get(new Engine());
            Assert.AreEqual(2, value2);
        }

        [TestMethod]
        public void SetItemTest()
        {
            var array = new List<int> { 1, 2, 3 };
            new SetItem { Context = array, Index = 1, Value = 42 }.Get(new Engine());
            Assert.AreEqual(42, array[1]);
            var list = new List<int> { 1, 2, 3 };
            new SetItem { Context = list, Index = 1, Value = 42 }.Get(new Engine());
            Assert.AreEqual(42, list[1]);
            var dictionary = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            new SetItem { Context = dictionary, Index = "b", Value = 42 }.Get(new Engine());
            Assert.AreEqual(42, dictionary["b"]);
        }
    }
}
