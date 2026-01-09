using MelonAccessibilityLib;

namespace DDLCScreenReaderMod
{
    /// <summary>
    /// Helper methods for text processing that supplement the library's TextCleaner.
    /// </summary>
    public static class TextHelper
    {
        /// <summary>
        /// Cleans text using the library's TextCleaner.
        /// </summary>
        public static string CleanText(string input)
        {
            return TextCleaner.Clean(input);
        }

        /// <summary>
        /// Determines if text appears to be narrative text (no speaker).
        /// </summary>
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
