﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming
{
    /// <summary>
    /// A BuiltinFunction is an intrinsic function provided
    /// by the markup programming engine.
    /// </summary>
    public enum BuiltinFunction
    {
        ParameterIsDefined = 1,
        FunctionIsDefined,
        GetResourceObject,
        Evaluate,
    }
}
