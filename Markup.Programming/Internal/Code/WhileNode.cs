namespace Markup.Programming.Core
{
    public class WhileNode : StatementNode
    {
        public StatementNode Body { get; set; }
        protected override void OnExecute(Engine engine)
        {
            engine.With(this, e => ExecuteWhile(engine));
        }

        private void ExecuteWhile(Engine engine)
        {
            engine.SetBreakFrame();
            while (TypeHelper.ConvertToBool(Context.Evaluate(engine)))
            {
                Body.Execute(engine);
                if (engine.ShouldInterrupt) break;
            }
        }
    }
}
