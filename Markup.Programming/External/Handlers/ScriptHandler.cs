using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    [ContentProperty("Script")]
    public class ScriptHandler : Handler
    {
        private enum CombinationType
        {
            None,
            PathOnly,
            ScriptOnly,
            PathAndScript,
        };

        public string Script { get; set; }

        private CodeTree scriptCodeTree = new CodeTree();
        protected CodeTree ScriptCodeTree { get { return scriptCodeTree; } }

        private CombinationType combinationType;

        protected override void OnActiveExecute(Engine engine)
        {
            if (Path != null)
            {
                combinationType = Script != null ? CombinationType.PathAndScript : CombinationType.PathOnly;
                RegisterEvent(engine, Path, CodeTree, CodeType.Expression);
            }
            else if (Script != null)
            {
                combinationType = CombinationType.ScriptOnly;
                RegisterEvent(engine, Script, ScriptCodeTree, CodeType.Statement);
            }
            else
            {
                combinationType = CombinationType.None;
                RegisterHandler(engine, null, null);
            }
            if (State == null) State = engine.GetClosure();
        }

        private void RegisterEvent(Engine engine, string path, CodeTree codeTree, CodeType codeType)
        {
            codeTree.Compile(engine, CodeType.Event | codeType, path);
            var context = codeTree.GetContext(engine);
            if (context == null) engine.Throw("context cannot be null for event: " + path);
            RegisterHandler(engine, context, codeTree.GetEvent(engine));
        }

        protected override void OnEventHandler(Engine engine)
        {
            engine.SetReturnFrame();
            switch (combinationType)
            {
                case CombinationType.None:
                    break;
                case CombinationType.PathOnly:
                    if (CodeTree.HasHandler) CodeTree.Get(engine);
                    break;
                case CombinationType.ScriptOnly:
                    if (ScriptCodeTree.HasHandler) ScriptCodeTree.Get(engine);
                    break;
                case CombinationType.PathAndScript:
                    engine.ExecuteScript(Script, CodeTree);
                    break;
            }
            engine.GetAndResetReturnValue();
        }
    }
}
