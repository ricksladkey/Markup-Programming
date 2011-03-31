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
            if (Path != null && Script != null)
            {
                combinationType = CombinationType.PathAndScript;
                CodeTree.Compile(engine, CodeType.Event, Path);
                var context = CodeTree.GetContext(engine);
                if (context == null) engine.Throw("context cannot be null for event: " + Path);
                RegisterHandler(engine, context, CodeTree.GetEvent(engine));
            }
            else if (Path != null)
            {
                combinationType = CombinationType.PathOnly;
                CodeTree.Compile(engine, CodeType.Event, Path);
                var context = CodeTree.GetContext(engine);
                if (context == null) engine.Throw("context cannot be null for event: " + Path);
                RegisterHandler(engine, context, CodeTree.GetEvent(engine));
            }
            else if (Script != null)
            {
                combinationType = CombinationType.ScriptOnly;
                ScriptCodeTree.Compile(engine, CodeType.Event | CodeType.Statement, Script);
                var context = ScriptCodeTree.GetContext(engine);
                if (context == null) engine.Throw("context cannot be null for event: " + Script);
                RegisterHandler(engine, context, ScriptCodeTree.GetEvent(engine));
            }
            else
            {
                combinationType = CombinationType.None;
                RegisterHandler(engine, null, null);
            }
            if (State == null) State = engine.GetClosure();
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
