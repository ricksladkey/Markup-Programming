using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodePlexApi;
using CookComputing.XmlRpc;
using Microsoft.Win32;
using System.Threading.Tasks;

namespace WikiWriter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new ViewModel();
            DataContext = ViewModel;
        }

        public ViewModel ViewModel { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.LoadArticlesAsync().ContinueWith(task =>
                {
                    ArticleSelector.ItemsSource = ViewModel.Articles;
                    ArticleSelector.SelectedItem = ViewModel.SelectedArticle;
                }, ui);
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
            ViewModel.UpdateSelectedArticleText(Text.Text);
            ViewModel.ProcessAsync(ViewModel.SelectedArticle).ContinueWith(task =>
                {
                    Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
                }, ui);
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateSelectedArticleText(Text.Text);
            if (!ViewModel.Save(ViewModel.SelectedArticle)) MessageBox.Show("Failed to save article.");
        }

        private void Publish(object sender, RoutedEventArgs e)
        {
            Save(sender, e);
            ViewModel.UpdateSelectedArticleText(Text.Text);
            for (int i = 0; i < ViewModel.Articles.Count; i++)
            {
                if (ViewModel.Articles[i] == ViewModel.SelectedArticle)
                {
                    currentPublishArticle = i;
                    limitPublishArticle = i + 1;
                    PublishOneArticle(null);
                    break;
                }
            }
            Browser.NavigateToString(ViewModel.SelectedArticle.HtmlWithHash);
        }

        private int currentPublishArticle;
        private int limitPublishArticle;

        private void PublishAll(object sender, RoutedEventArgs e)
        {
            currentPublishArticle = 0;
            limitPublishArticle = ViewModel.Articles.Count;
            PublishOneArticle(null);
        }

        private void PublishOneArticle(Task task)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            if (currentPublishArticle == limitPublishArticle) return;
            var article = ViewModel.Articles[currentPublishArticle];
            ViewModel.ProcessAsync(article)
                .ContinueWith(task1 => ViewModel.IsUpToDateAsync(article))
                .Unwrap<bool>().ContinueWith(task2 =>
                    {
                        if (task2.Result)
                        {
                            Status.Text = "Article up-to-date: " + article.Name;
                            ++currentPublishArticle;
                            PublishOneArticle(null);
                        }
                        else
                        {
                            ViewModel.PublishAsync(article).ContinueWith(task3 =>
                                {
                                    Status.Text = "Article published: " + article.Name;
                                    ++currentPublishArticle;
                                    PublishOneArticle(null);
                                }, ui);
                        }
                    }, ui);
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.EditAsync(ViewModel.SelectedArticle).ContinueWith(task =>
                {
                    if (!task.Result) MessageBox.Show("edit failed");
                    else Status.Text = "Checked out " + ViewModel.SelectedArticle.Name;
                }, ui);
        }

        private void Check(object sender, RoutedEventArgs e)
        {
            Preview(sender, e);
            Save(sender, e);
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            ViewModel.IsUpToDateAsync(ViewModel.SelectedArticle).ContinueWith(t =>
                {
                    if (t.Result) Status.Text = "Article up-to-date: " + ViewModel.SelectedArticle.Name;
                    else MessageBox.Show("Article is out-of-date: " + ViewModel.SelectedArticle.Name);
                }, ui);

        }

        private void CheckAll(object sender, RoutedEventArgs e)
        {
            currentPublishArticle = 0;
            limitPublishArticle = ViewModel.Articles.Count;
            CheckOneArticle(null);
        }

        private void CheckOneArticle(Task task)
        {
            if (currentPublishArticle == limitPublishArticle) return;
            var article = ViewModel.Articles[currentPublishArticle];
            var task1 = ViewModel.ProcessAsync(article);
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            var task2 = task1.ContinueWith(t1 => ViewModel.IsUpToDateAsync(article));
            task2.Unwrap<bool>().ContinueWith(t2 =>
            {
                Status.Text = string.Format("Article {0}: {1}", t2.Result ? "up-to-date" : "out-of-date", article.Name);
                ++currentPublishArticle;
                CheckOneArticle(null);
            }, ui);
        }
    }
}
