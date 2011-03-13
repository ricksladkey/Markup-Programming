using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Markup.Programming.Tests
{
    [TestClass]
    public class ControlFlowTests
    {
        [TestMethod]
        public void BreakTest()
        {
            // Create a infinite loop containing a break preceded by an statement and followed
            // an statement and check that only the former is executed.
            var viewModel = new BasicViewModel();
            Assert.AreEqual(null, viewModel.String1);
            Assert.AreEqual(null, viewModel.String2);
            Test.AttachAndExecute(new Window { DataContext = viewModel },
                new While
                {
                    Body =
                    {
                        Test.Configure(new Set { PropertyName = "String1", Value = "Test1" },
                            Test.TargetBinder),
                        new Break(),
                        Test.Configure(new Set { PropertyName = "String2", Value = "Test2" },
                            Test.TargetBinder),
                    }
                });
            Assert.AreEqual("Test1", viewModel.String1);
            Assert.AreEqual(null, viewModel.String2);
        }

        [TestMethod]
        public void InlineFunctionTest()
        {
            // Set X = 21 and then return X * 2 and check that the result is 42.
            Test.ExpressionTest(42,
                new InlineFunction
                {
                    Body =
                    {
                        new Set { ParameterName = "X", Value = 21 },
                        new Return
                        {
                            Value = new Op
                            {
                                Value1 = new Val { Path = "$X" },
                                Operator = Operator.Times,
                                Value2 = 2,
                            }
                        }
                    }
                });
        }

        [TestMethod]
        public void SwitchTest()
        {
            Test.ExpressionTest(42,
                new InlineFunction
                {
                    Body =
                    {
                        new Switch
                        {
                            Value = 2,
                            Body =
                            {
                                new Case { Value = 1 },
                                new Return { Value = 21 },
                                new Case { Value = 2 },
                                new Return { Value = 42 },
                                new Case { Value = 3 },
                                new Return { Value = 63 },
                            },
                            Default =
                            {
                                new Return { Value = 84 },
                            }
                        }
                    }
                });
        }
    }
}
