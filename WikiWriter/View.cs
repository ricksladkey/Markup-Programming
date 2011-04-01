using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WikiWriter
{
    public class View
    {
        public ComboBox ArticleSelector { get; set; }
        public TextBox Text { get; set; }
        public WebBrowser Browser { get; set; }
        public TextBlock Status { get; set; }

        public ViewModel ViewModel { get; set; }
        public IList<Article> PublishArticles { get; set; }
        public int CurrentPublishArticle { get; set; }
        public TaskScheduler Scheduler { get { return TaskScheduler.FromCurrentSynchronizationContext(); } }

        public void LoadArticles()
        {
            ViewModel.LoadArticlesAsync().ContinueWith(task =>
            {
                ArticleSelector.ItemsSource = ViewModel.Articles;
                ArticleSelector.SelectedItem = ViewModel.SelectedArticle;
            }, Scheduler);
        }

        public void SelectArticle(Article article)
        {
            ViewModel.SelectedArticle = article;
            Text.Text = ViewModel.SelectedArticle.Text;
            Preview();
        }

        public void Preview()
        {
            ViewModel.ProcessAsync(ViewModel.SelectedArticle).ContinueWith(task =>
            {
                Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
            }, Scheduler);
        }

        public void Publish()
        {
            Save();
            SetPublishArticles(new List<Article> { ViewModel.SelectedArticle });
            PublishOneArticle();
            Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
        }

        public void PublishAll()
        {
            PublishArticles = ViewModel.Articles;
            CurrentPublishArticle = 0;
            PublishOneArticle();
        }

        public void Check()
        {
            Save();
            SetPublishArticles(new List<Article> { ViewModel.SelectedArticle });
            CheckOneArticle();
            Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
        }

        public void CheckAll()
        {
            SetPublishArticles(ViewModel.Articles);
            CheckOneArticle();
        }

        public void Save()
        {
            if (!ViewModel.Save(ViewModel.SelectedArticle))
            {
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
                }, Scheduler);
            }
        }

        public void Edit()
        {
            ViewModel.EditAsync(ViewModel.SelectedArticle).ContinueWith(task =>
            {
                if (!task.Result) MessageBox.Show("Check out failed: " + ViewModel.SelectedArticle.Name);
                else Status.Text = "Checked out " + ViewModel.SelectedArticle.Name;
            }, Scheduler);
        }

        public void SetPublishArticles(IList<Article> articles)
        {
            CurrentPublishArticle = 0;
            PublishArticles = articles;
        }

        public void PublishOneArticle()
        {
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
                        }, Scheduler);
                    }
                }, Scheduler);
        }

        public void CheckOneArticle()
        {
            if (CurrentPublishArticle == PublishArticles.Count) return;
            var article = PublishArticles[CurrentPublishArticle];
            var task1 = ViewModel.ProcessAsync(article);
            var task2 = task1.ContinueWith(t1 => ViewModel.IsUpToDateAsync(article));
            task2.Unwrap<bool>().ContinueWith(t2 =>
            {
                Status.Text = string.Format("Article {0}: {1}", t2.Result ? "up-to-date" : "out-of-date", article.Name);
                ++CurrentPublishArticle;
                CheckOneArticle();
            }, Scheduler);
        }
    }
}
