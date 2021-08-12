using System.Text.RegularExpressions;

namespace Api.Models
{
    public static class TagHandler
    {
        public static bool IsTagVaild(this string tag)
        {
            var regex = new Regex("^[a-zA-Z0-9_-]*$");
            return regex.IsMatch(tag);
        }
    }
}