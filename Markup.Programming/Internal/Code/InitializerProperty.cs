using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public class InitializerProperty
    {
        public string PropertyName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsDictionary { get; set; }
        public ExpressionNode Value { get; set; }
        public IList<ExpressionNode> Values { get; set; }
    }
}
