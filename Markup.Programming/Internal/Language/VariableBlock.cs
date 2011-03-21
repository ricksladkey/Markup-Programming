using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Markup.Programming.Core
{
    public abstract class VariableBlock : ValueBlock
    {
        public string Var { get; set; }

        private CodeTree varCodeTree = new CodeTree();
        protected CodeTree VarCodeTree { get { return varCodeTree; } }
    }
}
