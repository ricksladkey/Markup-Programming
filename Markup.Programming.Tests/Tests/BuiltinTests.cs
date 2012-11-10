using System.Windows;
using Markup.Programming.Core;
using Xunit;

namespace Markup.Programming.Tests
{
    public class BuiltinTests
    {
        [Fact]
        public void IsDefinedTest()
        {
            TestHelper.ExpressionTest(true,
                new Call
                {
                    BuiltinFunction = BuiltinFunction.VariableIsDefined,
                    Arguments = { new Expr { Value = Engine.EventArgsKey } },
                });
            TestHelper.ExpressionTest(false,
                new Call
                {
                    BuiltinFunction = BuiltinFunction.VariableIsDefined,
                    Arguments = { new Expr { Value = "xyzzy" } },
                });
        }

        [Fact]
        public void GetResourceObjectTest()
        {
            var window = new Window
            {
                Resources =
                {
                    {
                        "ViewModel",
                        new ResourceObject
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
            Assert.Equal(viewModel, viewModel["ResourceObject"]);
        }
    }
}
