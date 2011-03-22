using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Continue statement continues to the nearest enclosing loop such as
    /// a For, While or Iterator.
    /// </summary>
    public class Continue : Statement
    {
        protected override void OnExecute(Engine engine)
        {
            engine.SetShouldContinue();
        }
    }
}
