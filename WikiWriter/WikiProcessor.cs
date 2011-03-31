using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace WikiWriter
{
    public class WikiProcessor
    {
        private enum Mode
        {
            Normal,
            List,
            Table,
        };

        private string Output { get; set; }
        private Mode CurrentMode { get; set; }

        private void SetMode(Mode mode)
        {
            if (mode == CurrentMode) return;
            if (CurrentMode == Mode.List) Output += "</ul>\r\n";
            if (CurrentMode == Mode.Table) Output += "</table>\r\n";
            if (mode == Mode.List) Output += "<ul>\r\n";
            if (mode == Mode.Table) Output += "<table>\r\n";
            CurrentMode = mode;
        }

        public string PHP { get; set; }

        public string Process(string input)
        {
            Output = "";
            var lines = input.Split('\n').Select(line => line.TrimEnd()).ToList();
            int i = 0;
            while (i < lines.Count)
            {
                var line = lines[i++];
                if (line.StartsWith("* "))
                {
                    SetMode(Mode.List);
                    Output += "<li>" + Subst(line.Substring(2)) + "</li>\r\n";
                    continue;
                }
                else if (line.StartsWith("| ") || line.StartsWith("|| "))
                {
                    SetMode(Mode.Table);
                    Output += "<tr>" + ProcessRow(line) + "</tr>\r\n";
                    continue;
                }
                else
                    SetMode(Mode.Normal);
                if (line.StartsWith("! "))
                {
                    Output += "<h1>" + Subst(line.Substring(2)) + "</h1>\r\n";
                    continue;
                }
                if (line.StartsWith("!! "))
                {
                    Output += "<h2>" + Subst(line.Substring(3)) + "</h2>\r\n";
                    continue;
                }
                if (line.StartsWith("!!! "))
                {
                    Output += "<h3>" + Subst(line.Substring(4)) + "</h3>\r\n";
                    continue;
                }
                if (line.StartsWith("!!!! "))
                {
                    Output += "<h4>" + Subst(line.Substring(5)) + "</h4>\r\n";
                    continue;
                }
                if (line.StartsWith("!!!!! "))
                {
                    Output += "<h5>" + Subst(line.Substring(6)) + "</h5>\r\n";
                    continue;
                }
                if (line.StartsWith("!!!!!! "))
                {
                    Output += "<h6>" + Subst(line.Substring(7)) + "</h6>\r\n";
                    continue;
                }
                if (line.StartsWith("----"))
                {
                    Output += "<hr />\r\n";
                    continue;
                }
                if (line == "{{")
                {
                    var code = new List<string>();
                    while (i < lines.Count)
                    {
                        var pre = lines[i++];
                        if (pre == "}}") break;
                        code.Add(pre);
                    }
                    Output += "<pre>\r\n";
                    foreach (var pre in code) Output += Quote(pre) + "\r\n";
                    Output += "</pre>\r\n";
                    continue;
                }
                if (line.StartsWith("{code:"))
                {
                    var language = line.Split(':')[1].Split('}')[0];
                    if (language == "html") language = "xml";
                    var code = new List<string>();
                    while (i < lines.Count)
                    {
                        var pre = lines[i++];
                        if (pre.StartsWith("{code:")) break;
                        code.Add(pre);
                    }
                    var inputFile = Path.GetTempFileName();
                    var outputFile = Path.GetTempFileName();
                    try
                    {
                        using (var streamWriter = new StreamWriter(inputFile))
                            foreach (var pre in code) streamWriter.WriteLine(pre);
                        var process = System.Diagnostics.Process.Start(new ProcessStartInfo
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            FileName = PHP,
                            Arguments = @"..\..\wiki.php" + " " + language + " " + "\"" + inputFile + "\"" + " " + "\"" + outputFile + "\"",
                        });
                        process.WaitForExit();
                        if (process.ExitCode != 0) throw new InvalidOperationException("PHP failed");
                        using (var streamReader = new StreamReader(outputFile))
                            Output += streamReader.ReadToEnd();
                    }
                    finally
                    {
                        File.Delete(inputFile);
                        File.Delete(outputFile);
                    }
                    continue;
                }
                Output += Subst(line) + "<br />\r\n";
            }
            return Output;
        }

        private string ProcessRow(string row)
        {
            bool header = row.StartsWith("||");
            var fields = (" " + row + " ").Replace(header ? " || " : " | ", "`").Split('`');
            var columns = fields.Skip(1).Take(fields.Length - 2);
            var tag = header ? "th" : "td";
            string output = "";
            foreach (var column in columns) output += string.Format("<{0}>{1}</{0}>", tag, Subst(column), tag);
            return output;
        }

        private string Quote(string input)
        {
            string output = "";
            foreach (char c in input)
            {
                switch (c)
                {
                    case '<':
                        output += "&lt;";
                        break;
                    case '>':
                        output += "&gt;";
                        break;
                    case '&':
                        output += "&amp;";
                        break;
                    default:
                        output += c;
                        break;
                }
            }
            return output;
        }

        private string Subst(string input)
        {
            var output = "";
            int i = 0;
            while (i < input.Length)
            {
                char c = input[i];
                if (c == '*')
                {
                    int start = ++i;
                    while (i < input.Length && input[i] != '*') ++i;
                    output += "<b>" + Subst(input.Substring(start, i - start)) + "</b>";
                    if (i < input.Length) ++i;
                    continue;
                }
                if (c == '_')
                {
                    int start = ++i;
                    while (i < input.Length && input[i] != '_') ++i;
                    output += "<i>" + Subst(input.Substring(start, i - start)) + "</i>";
                    if (i < input.Length) ++i;
                    continue;
                }
                if (c == '+')
                {
                    int start = ++i;
                    while (i < input.Length && input[i] != '+') ++i;
                    output += "<u>" + Subst(input.Substring(start, i - start)) + "</u>";
                    if (i < input.Length) ++i;
                    continue;
                }
                if (c == '[')
                {
                    int start = ++i;
                    while (i < input.Length && input[i] != ']') ++i;
                    var type = "local";
                    var page = input.Substring(start, i - start);
                    if (page.IndexOf(':') != -1)
                    {
                        type = page.Substring(0, page.IndexOf(':'));
                        page = page.Substring(page.IndexOf(':') + 1);
                    }
                    var link = page;
                    if (page.IndexOf('|') != -1)
                    {
                        link = page.Substring(0, page.IndexOf('|'));
                        page = page.Substring(page.IndexOf('|') + 1);
                    }
                    var href = "";
                    if (type == "url") href = page;
                    else if (type == "local") href = "/wikipage?title=" + page;
                    output += "<a href=\"" + href + "\">" + Subst(link) + "</a>";
                    if (i < input.Length) ++i;
                    continue;
                }
                if (i < input.Length - 1)
                {
                    var c2 = input.Substring(i, 2);
                    if (c2 == "~~")
                    {
                        int start = i += 2;
                        while (i < input.Length - 1 && input.Substring(i, 2) != "~~") ++i;
                        output += "<del>" + Subst(input.Substring(start, i - start)) + "</del>";
                        if (i < input.Length - 1) i += 2;
                        continue;
                    }
                    if (c2 == "^^")
                    {
                        int start = i += 2;
                        while (i < input.Length - 1 && input.Substring(i, 2) != "^^") ++i;
                        output += "<sup>" + Subst(input.Substring(start, i - start)) + "</sup>";
                        if (i < input.Length - 1) i += 2;
                        continue;
                    }
                    if (c2 == ",,")
                    {
                        int start = i += 2;
                        while (i < input.Length - 1 && input.Substring(i, 2) != ",,") ++i;
                        output += "<sub>" + Subst(input.Substring(start, i - start)) + "</sub>";
                        if (i < input.Length - 1) i += 2;
                        continue;
                    }
                    if (c2 == "{{")
                    {
                        int start = i += 2;
                        while (i < input.Length - 1 && input.Substring(i, 2) != "}}") ++i;
                        output += "<span class=\"codeInline\">" + input.Substring(start, i - start) + "</span>";
                        if (i < input.Length - 1) i += 2;
                        continue;
                    }
                    if (c2 == "{*")
                    {
                        int start = i += 2;
                        while (i < input.Length - 1 && input.Substring(i, 2) != "*}") ++i;
                        output += input.Substring(start, i - start);
                        if (i < input.Length - 1) i += 2;
                        continue;
                    }
                }
                output += c;
                ++i;
            }
            return output;
        }
    }
}
