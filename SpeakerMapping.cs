using System.Collections.Generic;

namespace DDLCScreenReaderMod
{
    public static class SpeakerMapping
    {
        private static readonly Dictionary<string, string> CharacterMap = new Dictionary<string, string>
        {
            { "s", "Sayori" },
            { "n", "Natsuki" },
            { "y", "Yuri" },
            { "m", "Monika" },
            { "mc", "MC" },  // Main Character
            { "dc", "" },    // Developer Commentary - filter out
            { "", "" }       // Narrator/empty
        };

        public static string GetSpeakerName(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "";

            return CharacterMap.TryGetValue(code.ToLower(), out string name) ? name : code;
        }

        public static bool ShouldFilterText(string text, string speakerCode)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Filter out developer commentary
            if (speakerCode == "dc")
                return true;

            // Filter out missing localization strings
            if (text.Contains("!MISSING") || text.Contains("MISSING"))
                return true;

            // Filter out technical strings
            if (text.Contains("Don't Skip Previous Line"))
                return true;

            return false;
        }
    }
}