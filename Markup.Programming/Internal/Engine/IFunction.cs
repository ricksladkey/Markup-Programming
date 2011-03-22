using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{
    public interface IFunction
    {
        void ExecuteBody(Engine engine);
        bool HasParamsParameter { get; }
        ParameterCollection Parameters { get; }
    }
}
