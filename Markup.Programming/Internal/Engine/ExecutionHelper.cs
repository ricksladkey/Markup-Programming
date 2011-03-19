using System.Windows;

namespace Markup.Programming.Core
{
    /// <summary>
    /// The ExecutionHelper class performs functions that would
    /// otherwise be performed in an executable base class if
    /// all executables could be derived from the same base class.
    /// Instead, classes that implement IExecutable forward their
    /// utility methods to this class for implementation.
    /// </summary>
    public static class ExecutionHelper
    {
        public static void Attach<TParent>(TParent parent, params DependencyProperty[] properties)
            where TParent : DependencyObject, IComponent
        {
            foreach (var property in properties)
            {
                if (PathHelper.HasBinding(parent, property)) continue;
                var value = parent.GetValue(property);
                if (value is ResourceComponent) continue;
                if (value is IComponent) (value as IComponent).Attach(parent.AssociatedObject);
            }
        }
    }
}
