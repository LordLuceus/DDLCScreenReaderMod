using System.Collections.Generic;
using DDLC;

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
            { "dc", "" },    // Developer Commentary - filter out
            { "", "" }       // Narrator/empty
        };

        public static string GetSpeakerName(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "";

            // Handle MC specially to use player's chosen name
            if (code.ToLower() == "mc")
            {
                return GetPlayerName();
            }

            return CharacterMap.TryGetValue(code.ToLower(), out string name) ? name : code;
        }

        private static string GetPlayerName()
        {
            try
            {
                // Try to get the player's chosen name from the game
                if (Renpy.CurrentContext != null)
                {
                    string playerName = Renpy.CurrentContext.GetVariableString("persistent.playername");
                    if (!string.IsNullOrEmpty(playerName))
                        return playerName;

                    // Fallback to the "player" variable
                    playerName = Renpy.CurrentContext.GetVariableString("player");
                    if (!string.IsNullOrEmpty(playerName))
                        return playerName;
                }
            }
            catch (System.Exception ex)
            {
                // If we can't access the player name, log and fallback
                ScreenReaderMod.Logger?.Warning($"Could not get player name: {ex.Message}");
            }

            // Fallback to generic "MC" if we can't get the player name
            return "MC";
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