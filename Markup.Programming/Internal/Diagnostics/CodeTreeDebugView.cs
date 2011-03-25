using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Markup.Programming.Core
{

#if DEBUG

    public class CodeTreeDebugView
    {
        private CodeTree codeTree;
        public CodeTreeDebugView(CodeTree codeTree) { this.codeTree = codeTree; }
        public int Count { get { return codeTree.Tokens.Count; } }
        public string Code { get { return codeTree.Code; } }
        public string Concat(IEnumerable<string> tokens) { return tokens.Aggregate("", (current, next) => current += " " + next); }
        public string Cursor
        {
            get
            {
                return
                    Concat(codeTree.Tokens.AllItems.Take(codeTree.Tokens.Current)) + " ^ " +
                    Concat(codeTree.Tokens.AllItems.Skip(codeTree.Tokens.Current));
            }
        }
    }

#endif

}
