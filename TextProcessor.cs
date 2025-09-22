using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DDLCScreenReaderMod
{
    public static class TextProcessor
    {
        private static readonly Regex RenpyTagRegex = new Regex(
            @"\{[^}]*\}",
            RegexOptions.Compiled
        );
        private static readonly Regex ColorTagRegex = new Regex(
            @"\[color=[^]]*\]|\[/color\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex SizeTagRegex = new Regex(
            @"\[size=[^]]*\]|\[/size\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex BoldTagRegex = new Regex(
            @"\[b\]|\[/b\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex ItalicTagRegex = new Regex(
            @"\[i\]|\[/i\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex UnderlineTagRegex = new Regex(
            @"\[u\]|\[/u\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex StrikeTagRegex = new Regex(
            @"\[s\]|\[/s\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex CenterTagRegex = new Regex(
            @"\[center\]|\[/center\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex RightTagRegex = new Regex(
            @"\[right\]|\[/right\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex LeftTagRegex = new Regex(
            @"\[left\]|\[/left\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex WaitTagRegex = new Regex(
            @"\{w[^}]*\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex NoWaitTagRegex = new Regex(
            @"\{nw\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex ClearTagRegex = new Regex(
            @"\{clear\}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex FontTagRegex = new Regex(
            @"\[font=[^]]*\]|\[/font\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex AlphaTagRegex = new Regex(
            @"\[alpha=[^]]*\]|\[/alpha\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly Regex HtmlItalicTagRegex = new Regex(
            @"<i>|</i>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex HtmlBoldTagRegex = new Regex(
            @"<b>|</b>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );
        private static readonly Regex HtmlUnderlineTagRegex = new Regex(
            @"<u>|</u>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        public static string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string cleaned = input;

            cleaned = RenpyTagRegex.Replace(cleaned, "");
            cleaned = ColorTagRegex.Replace(cleaned, "");
            cleaned = SizeTagRegex.Replace(cleaned, "");
            cleaned = BoldTagRegex.Replace(cleaned, "");
            cleaned = ItalicTagRegex.Replace(cleaned, "");
            cleaned = UnderlineTagRegex.Replace(cleaned, "");
            cleaned = StrikeTagRegex.Replace(cleaned, "");
            cleaned = CenterTagRegex.Replace(cleaned, "");
            cleaned = RightTagRegex.Replace(cleaned, "");
            cleaned = LeftTagRegex.Replace(cleaned, "");
            cleaned = WaitTagRegex.Replace(cleaned, "");
            cleaned = NoWaitTagRegex.Replace(cleaned, "");
            cleaned = ClearTagRegex.Replace(cleaned, "");
            cleaned = FontTagRegex.Replace(cleaned, "");
            cleaned = AlphaTagRegex.Replace(cleaned, "");

            cleaned = HtmlItalicTagRegex.Replace(cleaned, "");
            cleaned = HtmlBoldTagRegex.Replace(cleaned, "");
            cleaned = HtmlUnderlineTagRegex.Replace(cleaned, "");

            cleaned = UnescapeSpecialCharacters(cleaned);

            cleaned = cleaned.Replace("<br>", "\n").Replace("<br/>", "\n").Replace("<br />", "\n");
            cleaned = cleaned.Replace("<color=#000>", "").Replace("</color>", "");

            return cleaned;
        }

        private static string UnescapeSpecialCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder sb = new StringBuilder(input);

            sb.Replace("\\n", "\n");
            sb.Replace("\\r", "\r");
            sb.Replace("\\t", "\t");
            sb.Replace("\\\"", "\"");
            sb.Replace("\\'", "'");
            sb.Replace("\\\\", "\\");

            return sb.ToString();
        }

        public static string ExtractSpeakerName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (input.Contains(":"))
            {
                int colonIndex = input.IndexOf(':');
                if (colonIndex > 0 && colonIndex < input.Length - 1)
                {
                    string potentialSpeaker = input.Substring(0, colonIndex).Trim();
                    if (potentialSpeaker.Length > 0 && potentialSpeaker.Length < 50)
                    {
                        return CleanText(potentialSpeaker);
                    }
                }
            }

            return string.Empty;
        }

        public static bool IsSystemMessage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim().ToLower();

            return text.StartsWith("system:")
                || text.StartsWith("error:")
                || text.StartsWith("warning:")
                || text.StartsWith("info:")
                || text.Contains("loading")
                || text.Contains("saving")
                || text.Contains("achievement");
        }

        public static bool IsNarrativeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();

            return !text.Contains(":")
                || text.StartsWith("*")
                || text.StartsWith("(")
                || text.StartsWith("[");
        }
    }
}
