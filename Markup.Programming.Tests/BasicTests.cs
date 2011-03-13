using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using System.Windows.Data;
using Markup.Programming.Core;

namespace Markup.Programming.Tests
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void StandAloneTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            // Note: This test does not use any external methods.
            var viewModel = new MarkupObject
            {
                Properties =
                {
                    new Property { PropertyName = "String1", Value = "Test" },
                    new Property { PropertyName = "String2", Value = null },
                }
            }.Value as IProvideProperties;
            var window = new Window { DataContext = viewModel };
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual(null, viewModel["String2"]);
            var statement = new Set { PropertyName = "String2" };
            BindingOperations.SetBinding(statement, Set.ContextProperty, new Binding());
            BindingOperations.SetBinding(statement, Set.ValueProperty, new Binding("String1"));
            Attached.GetOperations(window).Add(new AttachedHandler { Actions = { statement } });
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual("Test", viewModel["String2"]);
        }

        [TestMethod]
        public void BasicBindingTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            var viewModel = new BasicViewModel { String1 = "Test" };
            Assert.AreEqual("Test", viewModel.String1);
            Assert.AreEqual(null, viewModel.String2);
            Test.AttachAndExecute(new Window { DataContext = viewModel },
                Test.Configure(new Set { PropertyName = "String2" },
                    Test.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))));
            Assert.AreEqual("Test", viewModel.String1);
            Assert.AreEqual("Test", viewModel.String2);
        }

        [TestMethod]
        public void IndirectBindingTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            var viewModel = new BasicViewModel { String1 = "Test" };
            Assert.AreEqual("Test", viewModel.String1);
            Assert.AreEqual(null, viewModel.String2);
            Test.AttachAndExecute(new Window { DataContext = viewModel },
                Test.Configure(new Set
                {
                    PropertyName = "String2",
                    Value = Test.Configure(new Val(),
                        value => BindingOperations.SetBinding(value, Val.ValueProperty, new Binding("String1"))),
                },
                Test.TargetBinder));
            Assert.AreEqual("Test", viewModel.String1);
            Assert.AreEqual("Test", viewModel.String2);
        }

        [TestMethod]
        public void NotifyPropertyChangedTest()
        {
            // Copy String1 to String2 and then copy String2 to String3.  If property
            // change notification isn't working, the binding for String2 will still
            // have its default value.
            var viewModel = new MarkupObject
            {
                Properties =
                    {
                        new Property { PropertyName = "String1", Value = "Test" },
                        new Property { PropertyName = "String2", Value = null },
                        new Property { PropertyName = "String3", Value = null },
                    }
            }.Value as IProvideProperties;
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual(null, viewModel["String2"]);
            Assert.AreEqual(null, viewModel["String3"]);
            Test.AttachAndExecute(new Window { DataContext = viewModel },
                Test.Configure(new Set { PropertyName = "String2" },
                    Test.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))),
                Test.Configure(new Set { PropertyName = "String3" },
                    Test.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String2"))));
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual("Test", viewModel["String2"]);
            Assert.AreEqual("Test", viewModel["String3"]);
        }

        [TestMethod]
        public void InheritanceContextTest()
        {
            // When MarkupObjects try to find their inheritance context,
            // it breaks data binding.
            var viewModel = new MarkupObject
            {
                Properties =
                    {
                        new Property { PropertyName = "String1", Value = "Test" },
                        new Property { PropertyName = "String2", Value = null },
                        new Property { PropertyName = "String3", Value = null },
                    }
            }.Value as IProvideProperties;
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual(null, viewModel["String2"]);
            Assert.AreEqual(null, viewModel["String3"]);
            var resource = new MarkupObject
            {
                Properties =
                    {
                        new Property { PropertyName = "Collection" },
                        new Property { PropertyName = "Item1" },
                    }
            };
            var window = new Window { DataContext = viewModel };
            window.Resources.Add(resource, "sampleData");
            Test.AttachAndExecute(window,
                Test.Configure(new Set { PropertyName = "String2" },
                    Test.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))),
                Test.Configure(new Set { PropertyName = "String3" },
                    Test.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String2"))));
            Assert.AreEqual("Test", viewModel["String1"]);
            Assert.AreEqual("Test", viewModel["String2"]);
            Assert.AreEqual("Test", viewModel["String3"]);
        }
    }
}
