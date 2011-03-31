using System;

namespace WikiWriter
{
    public class Article
    {
        public string Name { get; set; }
        public string Filename { get; set; }
        public string Text { get; set; }
        public DateTime TextTime { get; set; }
        public string Html { get; set; }
        public string HtmlWithHash { get; set; }
        public DateTime HtmlTime { get; set; }
    }
}
