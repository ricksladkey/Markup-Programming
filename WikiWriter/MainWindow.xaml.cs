using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WikiWriter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            View = new View
            {
                ArticleSelector = ArticleSelector,
                Text = Text,
                Browser = Browser,
                Status = Status
            };
            DataContext = View.ViewModel;
        }

        public View View { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            View.LoadArticles();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = View.SaveCommand.CanExecute(e.Parameter);
        }

        private void Save_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            View.SaveCommand.Execute(e.Parameter);
        }

        private void ArticleSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            View.SelectArticle(ArticleSelector.SelectedItem as Article);
        }

        private void Preview(object sender, RoutedEventArgs e)
        {
            View.Preview();
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            View.Save();
        }

        private void Publish(object sender, RoutedEventArgs e)
        {
            View.Publish();
        }

        private void PublishAll(object sender, RoutedEventArgs e)
        {
            View.PublishAll();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            View.Edit();
        }

        private void Check(object sender, RoutedEventArgs e)
        {
            View.Check();
        }

        private void CheckAll(object sender, RoutedEventArgs e)
        {
            View.CheckAll();
        }
    }
}
