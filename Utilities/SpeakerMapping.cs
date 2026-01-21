using DDLC;

namespace DDLCPlusAccess
{
    public static class SpeakerMapping
    {
        /// <summary>
        /// Gets the localized speaker name as displayed by the game.
        /// This matches what the game shows (e.g., "???" or "Girl 1" at the start).
        /// </summary>
        /// <param name="tag">The speaker tag/code (e.g., "s", "m", "mc")</param>
        /// <param name="character">Optional character parameter from RenpyWindowManager.Say.
        /// When provided, uses the game's exact localization logic.</param>
        public static string GetLocalizedSpeakerName(string tag, string character = null)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return "";

            try
            {
                // Handle MC specially to use player's chosen name
                if (tag.ToLower() == "mc")
                {
                    return GetPlayerName();
                }

                // Check if it's the player variable - use raw name
                string playerVar = Renpy.CurrentContext?.GetVariableString("player") ?? "";
                if (tag == playerVar)
                    return tag;

                // When character parameter is provided, use the game's exact logic
                // See RenpyWindowManager.Say lines 904-923
                if (character != null)
                {
                    if (!string.IsNullOrEmpty(character))
                    {
                        // Capitalize first letter: "s" -> "S"
                        string capitalizedTag = char.ToUpper(tag[0]) + tag.Substring(1);
                        string localized = Renpy.Text.GetLocalisedString(capitalizedTag);
                        if (localized != "MISSING!" && localized != "!MISSING")
                        {
                            return localized;
                        }
                        // Fall back to capitalized tag
                        return capitalizedTag;
                    }
                    else
                    {
                        // Character is empty string - try to localize tag directly
                        string localized = Renpy.Text.GetLocalisedString(tag);
                        if (localized != "MISSING!" && localized != "!MISSING")
                        {
                            return localized;
                        }
                        return tag;
                    }
                }

                // Fallback logic for when character parameter is not available (e.g., history)
                // Try capitalized version first
                string capitalizedCode = char.ToUpper(tag[0]) + tag.Substring(1);
                string localizedName = Renpy.Text.GetLocalisedString(capitalizedCode);
                if (
                    !string.IsNullOrEmpty(localizedName)
                    && localizedName != "MISSING!"
                    && localizedName != "!MISSING"
                )
                {
                    return localizedName;
                }

                // Try without capitalization
                localizedName = Renpy.Text.GetLocalisedString(tag);
                if (
                    !string.IsNullOrEmpty(localizedName)
                    && localizedName != "MISSING!"
                    && localizedName != "!MISSING"
                )
                {
                    return localizedName;
                }

                // Last resort: return the tag as-is
                return tag;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Warning(
                    $"Error getting localized speaker name: {ex.Message}"
                );
                return tag;
            }
        }

        public static string GetPlayerName()
        {
            try
            {
                // Try to get the player's chosen name from the game
                if (Renpy.CurrentContext != null)
                {
                    string playerName = Renpy.CurrentContext.GetVariableString(
                        "persistent.playername"
                    );
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
