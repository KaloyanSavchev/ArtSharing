using System.Text.RegularExpressions;

namespace ArtSharing.Web.Helpers 
{
    public static class ContentFilter
    {
        private static readonly List<string> BannedWords = new()
        {
            "fuck", "shit", "bitch", "asshole", "dick", "sex", "piss off", "dick head", "bastard", "cunt", "bollocks", "bugger", "chaod", "crikey", "rubbish", "shag", "wanker", "twat"
        };

        public static string CensorText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            string result = input;
            foreach (var word in BannedWords)
            {
                var pattern = new Regex($@"\b{Regex.Escape(word)}\b", RegexOptions.IgnoreCase);
                result = pattern.Replace(result, m =>
                {
                    var first = m.Value[0];
                    var censored = first + new string('*', m.Value.Length - 1);
                    return censored;
                });
            }

            return result;
        }
    }
}
