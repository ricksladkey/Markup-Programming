using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// A MarkupCommand is an ICommand that can be defined and
    /// referenced entirely in resources and by IExpression
    /// and IStatement to implement the CanExecute and Execute
    /// interface methods.
    /// </summary>
    [ContentProperty("ExecuteBody")]
    public class MarkupCommand : ResourceObject, ICommand
    {
        public MarkupCommand()
        {
            LoadActions = new StatementCollection();
            ExecuteBody = new StatementCollection();
        }

        public StatementCollection LoadActions
        {
            get { return (StatementCollection)GetValue(LoadActionsProperty); }
            set { SetValue(LoadActionsProperty, value); }
        }

        public static readonly DependencyProperty LoadActionsProperty =
            DependencyProperty.Register("LoadActions", typeof(StatementCollection), typeof(MarkupCommand), null);

        public IExpression CanExecuteExpression
        {
            get { return (IExpression)GetValue(CanExecuteExpressionProperty); }
            set { SetValue(CanExecuteExpressionProperty, value); }
        }

        public static readonly DependencyProperty CanExecuteExpressionProperty =
            DependencyProperty.Register("CanExecutExpression", typeof(IExpression), typeof(MarkupCommand), null);
        
        public StatementCollection ExecuteBody
        {
            get { return (StatementCollection)GetValue(ExecuteBodyProperty); }
            set { SetValue(ExecuteBodyProperty, value); }
        }

        public static readonly DependencyProperty ExecuteBodyProperty =
            DependencyProperty.Register("ExecuteBody", typeof(StatementCollection), typeof(MarkupCommand), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(LoadActionsProperty, CanExecuteExpressionProperty, ExecuteBodyProperty);
            var parameters = new NameDictionary { { "@Command", this } };
            new Engine().With(this, parameters, engine => LoadActions.Execute(engine));
        }

        public bool CanExecute(object parameter)
        {
            TryToAttach();
            if (CanExecuteExpression == null) return true;
            var parameters = new NameDictionary{ { "@CommandParameter", parameter} };
            return new Engine(parameter).With(this, parameters,
                engine => (bool)engine.Evaluate(CanExecuteExpressionProperty));
        }

#if SILVERLIGHT
        public event System.EventHandler CanExecuteChanged;
        private void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null) CanExecuteChanged(this, new EventArgs());
        }
#else
        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
#endif

        public void Execute(object parameter)
        {
            TryToAttach();
            var parameters = new NameDictionary { { "@CommandParameter", parameter } };
            new Engine(parameter).With(this, parameters, engine => ExecuteBody.Execute(engine));
        }
    }
}
