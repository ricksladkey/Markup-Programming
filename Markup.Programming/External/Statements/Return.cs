﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Markup.Programming.Core;
using System.Windows;
using System.Windows.Markup;

namespace Markup.Programming
{
    /// <summary>
    /// The Return statement causes the currently executing Function
    /// or InlineFunction to return.  The value that is returned
    /// can be omitted or it will return Value or ValueParam,
    /// optionally converted to Type.
    /// </summary>
    public class Return : ConditionalValueStatement
    {
        protected override void OnExecute(Engine engine)
        {
            if (!ShouldExecute(engine)) return;
            var type = engine.GetType(TypeProperty, TypePath, TypeCodeTree);
            var value = engine.Get(ValueProperty, Path, CodeTree, type);
            engine.SetReturnValue(value);
        }
    }
}
