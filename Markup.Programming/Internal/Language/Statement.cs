using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Markup.Programming.Core
{
    /// <summary>
    /// A Statement is an IProcessor that processes by
    /// setting its context and calling its OnExecute
    /// override.
    /// </summary>
    public abstract class Statement : StatementBase, IStatement
    {
        protected override object OnProcess(Engine engine)
        {
            engine.SetContext(ContextProperty, ContextPath, ContextCodeTree);
            OnExecute(engine);
            return null;
        }

        protected abstract void OnExecute(Engine engine);
    }
}
