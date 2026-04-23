using System.Text;
using System.Collections.Generic;

namespace Orchestrate.Models
{
    public class HtmlPage
    {
        public List<string> Metadatas { get; } = new List<string>();
        public List<string> Styles { get; } = new List<string>();
        public List<string> Scripts { get; } = new List<string>();
        public string Content { get; set; } = "";
        public string Render()
        {
            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE HTML><html lang=\"en\"><head>");
            sb.Append(string.Join("", Metadatas));
            sb.Append(string.Join("", this.Styles));
            sb.Append("</head><body>");
            sb.Append(this.Content);
            sb.Append(string.Join("", this.Scripts));
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
    public static class Layout
    {
        private static readonly Dictionary<string, HtmlPage> _dict;
        static Layout()
        {
            _dict = new Dictionary<string, HtmlPage>();
            _dict["main"] = new HtmlPage();
        }

        public static HtmlPage Main => _dict["main"];

        public static HtmlPage GetOrCreate(string name)
        {
            if (!_dict.TryGetValue(name, out var page))
            {
                page = new HtmlPage();
                _dict[name] = page;
            }
            return page;
        }
        public static string GetPage(string name, string content)
        {
            var page = GetOrCreate(name);
            page.Content = content;
            return page.Render();
        }
    }
}
