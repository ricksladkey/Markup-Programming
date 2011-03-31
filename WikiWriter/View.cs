using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WikiWriter
{
    public class View
    {
        public View()
        {
            ViewModel = new ViewModel();
            SaveCommand = new RelayCommand(p => true, p => Save());
        }

        public TextBlock Status { get; set; }
        public ICommand SaveCommand { get; set; }

        public ViewModel ViewModel { get; set; }
        public IList<Article> PublishArticles { get; set; }
        public int CurrentPublishArticle { get; set; }

        public void Save()
        {
            if (!ViewModel.Save(ViewModel.SelectedArticle))
            {
                var ui = TaskScheduler.FromCurrentSynchronizationContext();
                ViewModel.EditAsync(ViewModel.SelectedArticle).ContinueWith(task =>
                {
                    if (!task.Result) MessageBox.Show("Check out failed: " + ViewModel.SelectedArticle.Name);
                    else
                    {
                        if (!ViewModel.Save(ViewModel.SelectedArticle))
                            MessageBox.Show("Failed to save article: " + ViewModel.SelectedArticle.Name);
                        else
                            Status.Text = "Checked out: " + ViewModel.SelectedArticle.Name;
                    }
                }, ui);
            }
        }

        public void Edit()
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.EditAsync(ViewModel.SelectedArticle).ContinueWith(task =>
            {
                if (!task.Result) MessageBox.Show("Check out failed: " + ViewModel.SelectedArticle.Name);
                else Status.Text = "Checked out " + ViewModel.SelectedArticle.Name;
            }, ui);
        }

        public void SetPublishArticles(IList<Article> articles)
        {
            CurrentPublishArticle = 0;
            PublishArticles = articles;
        }

        public void PublishOneArticle()
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            if (CurrentPublishArticle == PublishArticles.Count) return;
            var article = PublishArticles[CurrentPublishArticle];
            ViewModel.ProcessAsync(article)
                .ContinueWith(task1 => ViewModel.IsUpToDateAsync(article))
                .Unwrap<bool>().ContinueWith(task2 =>
                {
                    if (task2.Result)
                    {
                        Status.Text = "Article up-to-date: " + article.Name;
                        ++CurrentPublishArticle;
                        PublishOneArticle();
                    }
                    else
                    {
                        ViewModel.PublishAsync(article).ContinueWith(task3 =>
                        {
                            Status.Text = "Article published: " + article.Name;
                            ++CurrentPublishArticle;
                            PublishOneArticle();
                        }, ui);
                    }
                }, ui);
        }

        public void CheckOneArticle()
        {
            if (CurrentPublishArticle == PublishArticles.Count) return;
            var article = PublishArticles[CurrentPublishArticle];
            var task1 = ViewModel.ProcessAsync(article);
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task2 = task1.ContinueWith(t1 => ViewModel.IsUpToDateAsync(article));
            task2.Unwrap<bool>().ContinueWith(t2 =>
            {
                Status.Text = string.Format("Article {0}: {1}", t2.Result ? "up-to-date" : "out-of-date", article.Name);
                ++CurrentPublishArticle;
                CheckOneArticle();
            }, ui);
        }
    }
}
