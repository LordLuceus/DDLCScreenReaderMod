using System.Collections.Generic;
using LibTextType = UnityAccessibilityLib.TextType;

namespace DDLCPlusAccess
{
    /// <summary>
    /// Defines text type constants that map to MelonAccessibilityLib's TextType system.
    /// </summary>
    public static class GameTextType
    {
        // Base types from library
        public const int Dialogue = LibTextType.Dialogue; // 0
        public const int Narrator = LibTextType.Narrator; // 1
        public const int Menu = LibTextType.Menu; // 2
        public const int MenuChoice = LibTextType.MenuChoice; // 3
        public const int SystemMessage = LibTextType.System; // 4

        // Game-specific custom types
        public const int PoetryGame = LibTextType.CustomBase + 1; // 101
        public const int FileBrowser = LibTextType.CustomBase + 2; // 102
        public const int Poem = LibTextType.CustomBase + 3; // 103
        public const int Settings = LibTextType.CustomBase + 4; // 104
        public const int Mail = LibTextType.CustomBase + 5; // 105
        public const int Jukebox = LibTextType.CustomBase + 6; // 106

        /// <summary>
        /// Gets a dictionary mapping text type IDs to readable names for logging.
        /// </summary>
        public static Dictionary<int, string> GetTextTypeNames()
        {
            return new Dictionary<int, string>
            {
                { Dialogue, "Dialogue" },
                { Narrator, "Narrator" },
                { Menu, "Menu" },
                { MenuChoice, "MenuChoice" },
                { SystemMessage, "System" },
                { PoetryGame, "PoetryGame" },
                { FileBrowser, "FileBrowser" },
                { Poem, "Poem" },
                { Settings, "Settings" },
                { Mail, "Mail" },
                { Jukebox, "Jukebox" },
            };
        }

        /// <summary>
        /// Predicate that determines which text types should be stored for repeat functionality.
        /// </summary>
        public static bool ShouldStoreForRepeat(int textType)
        {
            return textType == Dialogue || textType == Narrator || textType == Poem;
        }

        /// <summary>
        /// Maps the legacy TextType enum to the new GameTextType constants.
        /// </summary>
        public static int FromLegacyTextType(TextType textType)
        {
            return textType switch
            {
                TextType.Dialogue => Dialogue,
                TextType.MenuChoice => MenuChoice,
                TextType.Menu => Menu,
                TextType.SystemMessage => SystemMessage,
                TextType.Narrator => Narrator,
                TextType.PoetryGame => PoetryGame,
                TextType.FileBrowser => FileBrowser,
                TextType.Poem => Poem,
                TextType.Settings => Settings,
                TextType.Mail => Mail,
                TextType.Jukebox => Jukebox,
                _ => SystemMessage,
            };
        }
    }

    /// <summary>
    /// Legacy TextType enum for backward compatibility with existing patches.
    /// </summary>
    public enum TextType
    {
        Dialogue,
        MenuChoice,
        Menu,
        SystemMessage,
        Narrator,
        PoetryGame,
        FileBrowser,
        Poem,
        Settings,
        Mail,
        Jukebox,
    }
}
