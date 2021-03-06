﻿using System.Windows;
using System.Windows.Data;
using Markup.Programming.Core;
using Xunit;

namespace Markup.Programming.Tests
{
    public class BasicTests
    {
        [Fact]
        public void StandAloneTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            // Note: This test does not use any external methods.
            var viewModel = new ResourceObject
            {
                Properties =
                {
                    new Property { PropertyName = "String1", Value = "Test" },
                    new Property { PropertyName = "String2", Value = null },
                }
            }.Value as IDynamicObject;
            var window = new Window { DataContext = viewModel };
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal(null, viewModel["String2"]);
            var statement = new Set { PropertyName = "String2" };
            BindingOperations.SetBinding(statement, Set.ContextProperty, new Binding());
            BindingOperations.SetBinding(statement, Set.ValueProperty, new Binding("String1"));
            Attached.GetOperations(window).Add(new AttachedHandler { Actions = { statement } });
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal("Test", viewModel["String2"]);
        }

        [Fact]
        public void BasicBindingTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            var viewModel = new BasicViewModel { String1 = "Test" };
            Assert.Equal("Test", viewModel.String1);
            Assert.Equal(null, viewModel.String2);
            TestHelper.AttachAndExecute(new Window { DataContext = viewModel },
                TestHelper.Configure(new Set { PropertyName = "String2" },
                    TestHelper.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))));
            Assert.Equal("Test", viewModel.String1);
            Assert.Equal("Test", viewModel.String2);
        }

        [Fact]
        public void IndirectBindingTest()
        {
            // Use Set to copy String1 to String2 using the DataContext of a Window.
            var viewModel = new BasicViewModel { String1 = "Test" };
            Assert.Equal("Test", viewModel.String1);
            Assert.Equal(null, viewModel.String2);
            TestHelper.AttachAndExecute(new Window { DataContext = viewModel },
                TestHelper.Configure(new Set
                {
                    PropertyName = "String2",
                    Value = TestHelper.Configure(new Expr(),
                        value => BindingOperations.SetBinding(value, Expr.ValueProperty, new Binding("String1"))),
                },
                TestHelper.TargetBinder));
            Assert.Equal("Test", viewModel.String1);
            Assert.Equal("Test", viewModel.String2);
        }

        [Fact]
        public void NotifyPropertyChangedTest()
        {
            // Copy String1 to String2 and then copy String2 to String3.  If property
            // change notification isn't working, the binding for String2 will still
            // have its default value.
            var viewModel = new ResourceObject
            {
                Properties =
                    {
                        new Property { PropertyName = "String1", Value = "Test" },
                        new Property { PropertyName = "String2", Value = null },
                        new Property { PropertyName = "String3", Value = null },
                    }
            }.Value as IDynamicObject;
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal(null, viewModel["String2"]);
            Assert.Equal(null, viewModel["String3"]);
            TestHelper.AttachAndExecute(new Window { DataContext = viewModel },
                TestHelper.Configure(new Set { PropertyName = "String2" },
                    TestHelper.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))),
                TestHelper.Configure(new Set { PropertyName = "String3" },
                    TestHelper.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String2"))));
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal("Test", viewModel["String2"]);
            Assert.Equal("Test", viewModel["String3"]);
        }

        [Fact]
        public void InheritanceContextTest()
        {
            // When MarkupObjects try to find their inheritance context,
            // it breaks data binding.
            var viewModel = new ResourceObject
            {
                Properties =
                    {
                        new Property { PropertyName = "String1", Value = "Test" },
                        new Property { PropertyName = "String2", Value = null },
                        new Property { PropertyName = "String3", Value = null },
                    }
            }.Value as IDynamicObject;
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal(null, viewModel["String2"]);
            Assert.Equal(null, viewModel["String3"]);
            var resource = new ResourceObject
            {
                Properties =
                    {
                        new Property { PropertyName = "Collection" },
                        new Property { PropertyName = "Item1" },
                    }
            };
            var window = new Window { DataContext = viewModel };
            window.Resources.Add(resource, "sampleData");
            TestHelper.AttachAndExecute(window,
                TestHelper.Configure(new Set { PropertyName = "String2" },
                    TestHelper.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String1"))),
                TestHelper.Configure(new Set { PropertyName = "String3" },
                    TestHelper.TargetBinder,
                    value => BindingOperations.SetBinding(value, Set.ValueProperty, new Binding("String2"))));
            Assert.Equal("Test", viewModel["String1"]);
            Assert.Equal("Test", viewModel["String2"]);
            Assert.Equal("Test", viewModel["String3"]);
        }
    }
}
