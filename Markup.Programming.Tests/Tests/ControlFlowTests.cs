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
            TestHelper.AttachAndExecute(new Window { DataContext = viewModel },
                new While
                {
                    Body =
                    {
                        TestHelper.Configure(new Set { PropertyName = "String1", Value = "Test1" },
                            TestHelper.TargetBinder),
                        new Break(),
                        TestHelper.Configure(new Set { PropertyName = "String2", Value = "Test2" },
                            TestHelper.TargetBinder),
                    }
                });
            Assert.AreEqual("Test1", viewModel.String1);
            Assert.AreEqual(null, viewModel.String2);
        }

        [TestMethod]
        public void InlineFunctionTest()
        {
            // Set X = 21 and then return X * 2 and check that the result is 42.
            TestHelper.ExpressionTest(42,
                new InlineFunction
                {
                    Body =
                    {
                        new Set { VariableName = "X", Value = 21 },
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
            TestHelper.ExpressionTest(42,
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
