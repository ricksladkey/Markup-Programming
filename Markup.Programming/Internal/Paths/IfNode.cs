namespace Markup.Programming.Core
{
    public class IfNode : StatementNode
    {
        public StatementNode Then { get; set; }
        public StatementNode Else { get; set; }
        protected override void OnExecute(Engine engine)
        {
            if (TypeHelper.ConvertToBool(Context.Evaluate(engine))) Then.Execute(engine);
            else if (Else != null) Else.Execute(engine);
        }
    }
}
