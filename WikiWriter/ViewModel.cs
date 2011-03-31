using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CodePlexApi;

namespace WikiWriter
{
    public class ViewModel
    {
        private Article selectedArticle;
        private string selectedText;

        public ViewModel()
        {
        }

        private Task StartNewTask(Action action)
        {
            return Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        private Task<TResult> StartNewTask<TResult>(Func<TResult> func)
        {
            return Task.Factory.StartNew<TResult>(func, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        public static string RegistryKey = @"Software\WikiWriter";

        public IDictionary<string, object> Storage { get; set; }

        public List<Article> Articles { get; set; }
        public List<Post> Posts { get; set; }
        public Article SelectedArticle
        {
            get { return selectedArticle; }
            set { selectedArticle = value; OnSelectedArticleChanged(); }
        }
        public string SelectedText
        {
            get { return selectedText; }
            set { selectedText = value; OnSelectedTextChanged(); }
        }

        private void OnSelectedArticleChanged()
        {
            Storage["SelectedArticle"] = SelectedArticle.Name;
            Utils.SetUserRegistry(RegistryKey, Storage);
        }

        private void OnSelectedTextChanged()
        {
            if (SelectedArticle.Text != SelectedText)
            {
                SelectedArticle.Text = SelectedText;
                SelectedArticle.TextTime = DateTime.Now;
            }
        }

        private void LoadArticles()
        {
            Articles = Utils.GetFilesRecursive(@"..\..\..\Markup.Programming.Documentation\wiki")
                .Select(filename => new Article
                {
                    Filename = filename,
                    Name = filename.Substring(filename.LastIndexOf('\\') + 1).Split('.')[0],
                    Text = Utils.ReadFile(filename),
                    TextTime = DateTime.Now,
                })
                .OrderBy(article => article.Name).ToList();
            Storage = Utils.GetUserRegistry(RegistryKey);
            if (Storage.ContainsKey("SelectedArticle"))
            {
                var name = Storage["SelectedArticle"] as string;
                foreach (var article in Articles) if (article.Name == name) SelectedArticle = article;
            }
            GetPostIds();
        }

        private void GetPostIds()
        {
            try
            {
                CodePlexMetaWeblog mw = new CodePlexMetaWeblog();
                string username = Storage["Username"] as string;
                string password = Storage["Password"] as string;
                string blogid = Storage["BlogId"] as string;
                Posts = mw.getRecentPosts(blogid, username, password, int.MaxValue).ToList();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Unknown error: " + exception.Message);
            }
        }

        public Task LoadArticlesAsync()
        {
            return StartNewTask(LoadArticles);
        }

        private bool IsUpToDate(Article article)
        {
            foreach (var p in Posts)
            {
                if (p.title == article.Name)
                {
                    var postid = Convert.ToString(p.postid);
                    CodePlexMetaWeblog mw = new CodePlexMetaWeblog();
                    string username = Storage["Username"] as string;
                    string password = Storage["Password"] as string;
                    string blogid = Storage["BlogId"] as string;
                    var post = mw.getPost(postid, username, password);
                    var index = post.description.IndexOf("@hash=");
                    if (index != -1)
                    {
                        var rest = post.description.Substring(index + "@hash=".Length);
                        var hash = rest.Substring(0, rest.IndexOf('@'));
                        var myHash = Utils.GetHash(article.Html);
                        return hash == myHash;
                    }
                }
            }
            return false;
        }

        public Task<bool> IsUpToDateAsync(Article article)
        {
            return StartNewTask<bool>(() => IsUpToDate(article));
        }

        private void Process(Article article)
        {
            if (article.HtmlTime > article.TextTime) return;
            article.HtmlTime = DateTime.Now;
            article.Html = new WikiProcessor { PHP = Storage["PHP"] as string }.Process(article.Text);
            string hash = "@hash=" + Utils.GetHash(article.Html) + "@";
            article.HtmlWithHash = article.Html + "<span style=\"color:white; background:white;\">" + hash + "</span>\r\n";
        }

        public Task ProcessAsync(Article article)
        {
            return StartNewTask(() => Process(article));
        }

        private void Publish(Article article)
        {
            CodePlexMetaWeblog mw = new CodePlexMetaWeblog();
            string username = Storage["Username"] as string;
            string password = Storage["Password"] as string;
            string blogid = Storage["BlogId"] as string;
            foreach (var p in Posts)
            {
                if (p.title == article.Name)
                {
                    var postid = Convert.ToString(p.postid);
                    var post = mw.getPost(postid, username, password);
                    post.description = article.HtmlWithHash;
                    mw.editPost(postid, username, password, post, true);
                    return;
                }
            }
            {
                var post = new Post
                {
                    categories = new string[0],
                    dateCreated = DateTime.Now,
                    title = article.Name,
                    description = article.HtmlWithHash,
                };
                var postid = mw.newPost(blogid, username, password, post, true);
                Posts.Add(mw.getPost(postid, username, password));
            }
        }

        public Task PublishAsync(Article article)
        {
            return StartNewTask(() => Publish(article));
        }

        private bool Edit(Article article)
        {
            var process = System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = Path.Combine(Environment.GetEnvironmentVariable("VS100COMNTOOLS"), @"..\ide\tf.exe"),
                Arguments = "edit" + " " + "\"" + article.Filename + "\"",
            });
            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public Task<bool> EditAsync(Article article)
        {
            return StartNewTask<bool>(() => Edit(article));
        }

        public bool Save(Article article)
        {
            if (Utils.ReadFile(article.Filename) == article.Text) return true;
            if (new FileInfo(article.Filename).IsReadOnly) return false;
            Utils.WriteFile(article.Filename, article.Text);
            return true;
        }
    }
}
