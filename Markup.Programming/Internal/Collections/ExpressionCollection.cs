﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Markup.Programming.Core;
using System.Diagnostics;

namespace Markup.Programming.Core
{
    /// <summary>
    /// An ExpressionCollection is a collection ExpressionBase.  Ideally
    /// this would be a collection of IExpression but FreezableCollection
    /// doesn't allow that.
    /// </summary>
    public class ExpressionCollection : ComponentCollection<ExpressionBase>
    {
        public object[] Get(Engine engine)
        {
            return this.Select(expression => expression.Get(engine)).ToArray();
        }
    }
}
