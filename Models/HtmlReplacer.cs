using System.Collections.Generic;

namespace Api.Models
{
    /// <summary>
    /// Lớp xử lý thay thế giá trị thích hợp vào mẫu Html sẵn có.
    /// </summary>
    public static class HtmlReplacer
    {
        public static string HtmlReplace(this string html, Dictionary<string, string> pairs)
        {
            foreach (var item in pairs)
            {
                html = html.Replace(item.Key, item.Value);
            }

            return html;
        }
    }
}
