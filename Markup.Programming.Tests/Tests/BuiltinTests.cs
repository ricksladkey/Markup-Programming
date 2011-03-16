﻿using System.Windows;
using Markup.Programming.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Markup.Programming.Tests
{
    [TestClass]
    public class BuiltinTests
    {
        [TestMethod]
        public void IsDefinedTest()
        {
            TestHelper.ExpressionTest(true,
                new Call
                {
                    BuiltinFunction = BuiltinFunction.ParameterIsDefined,
                    Arguments = { new Val { Value = "@EventArgs" } },
                });
            TestHelper.ExpressionTest(false,
                new Call
                {
                    BuiltinFunction = BuiltinFunction.ParameterIsDefined,
                    Arguments = { new Val { Value = "xyzzy" } },
                });
        }

        [TestMethod]
        public void GetResourceObjectTest()
        {
            var window = new Window
            {
                Resources =
                {
                    {
                        "ViewModel",
                        new MarkupObject
                        {
                            Properties =
                            {
                                new Property
                                {
                                    PropertyName = "ResourceObject",
                                    Value = new Call { BuiltinFunction = BuiltinFunction.GetResourceObject },
                                }
                            }
                        }
                    }
                }
            };
            var viewModel = window.FindResource("ViewModel") as IDynamicObject;
            Assert.AreEqual(viewModel, viewModel["ResourceObject"]);
        }
    }
}