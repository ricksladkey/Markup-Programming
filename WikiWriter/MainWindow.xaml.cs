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
            View = new View { Status = Status };
            DataContext = View.ViewModel;
        }

        public View View { get; set; }
        public ViewModel ViewModel { get { return View.ViewModel; } }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.LoadArticlesAsync().ContinueWith(task =>
                {
                    ArticleSelector.ItemsSource = ViewModel.Articles;
                    ArticleSelector.SelectedItem = ViewModel.SelectedArticle;
                }, ui);
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
            ViewModel.SelectedArticle = ArticleSelector.SelectedItem as Article;
            Text.Text = ViewModel.SelectedArticle.Text;
            Preview(sender, e);
        }

        private void Preview(object sender, RoutedEventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.ProcessAsync(ViewModel.SelectedArticle).ContinueWith(task =>
                {
                    Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
                }, ui);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            View.Save();
        }

        private void Publish(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
            View.SetPublishArticles(new List<Article> { ViewModel.SelectedArticle });
            View.PublishOneArticle();
            Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
        }

        private void PublishAll(object sender, RoutedEventArgs e)
        {
            View.PublishArticles = ViewModel.Articles;
            View.CurrentPublishArticle = 0;
            View.PublishOneArticle();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            View.Edit();
        }

        private void Check(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
            View.SetPublishArticles(new List<Article> { ViewModel.SelectedArticle });
            View.CheckOneArticle();
            Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
        }

        private void CheckAll(object sender, RoutedEventArgs e)
        {
            View.SetPublishArticles(ViewModel.Articles);
            View.CheckOneArticle();
        }
    }
}
