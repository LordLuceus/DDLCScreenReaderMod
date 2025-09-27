using System.Collections.Generic;
using RenpyParser;

namespace DDLCScreenReaderMod
{
    public static class PreferenceTypeNames
    {
        private static readonly Dictionary<PreferenceType, string> PreferenceNames = new Dictionary<
            PreferenceType,
            string
        >
        {
            { PreferenceType.TextSpeed, "Text speed" },
            { PreferenceType.AutoForwardTime, "Auto forward time" },
            { PreferenceType.MusicVolume, "Music volume" },
            { PreferenceType.SoundVolume, "Sound volume" },
            { PreferenceType.DeveloperCommentary, "Developer commentary" },
            { PreferenceType.SkipUnseenText, "Skip unseen text" },
            { PreferenceType.SkipAfterChoices, "Skip after choices" },
            { PreferenceType.Window, "Window mode" },
            { PreferenceType.Fullscreen, "Fullscreen" },
            { PreferenceType.MuteAll, "Mute all" },
            { PreferenceType.DeveloperCommentaryVolume, "Developer commentary volume" },
            { PreferenceType.DeveloperCommentaryAutoplayAudio, "Developer commentary autoplay" },
            { PreferenceType.EnableContentWarnings, "Content warnings" },
            { PreferenceType.Language, "Language" },
            { PreferenceType.VSync, "VSync" },
            { PreferenceType.DialogueBoxSize, "Dialogue box size" },
        };

        public static string GetFriendlyName(PreferenceType preferenceType)
        {
            return PreferenceNames.TryGetValue(preferenceType, out string name)
                ? name
                : preferenceType.ToString();
        }

        public static string FormatSliderValue(float value, PreferenceType preferenceType)
        {
            switch (preferenceType)
            {
                case PreferenceType.MusicVolume:
                case PreferenceType.SoundVolume:
                case PreferenceType.DeveloperCommentaryVolume:
                    return $"{(int)(value * 100)}%";

                case PreferenceType.TextSpeed:
                    return $"{(int)value} characters per second";

                case PreferenceType.AutoForwardTime:
                    return $"{value:F1} seconds";

                case PreferenceType.DialogueBoxSize:
                    return $"{(int)(value * 100)}%";

                default:
                    return value.ToString("F1");
            }
        }

        public static string FormatToggleState(bool isEnabled)
        {
            return isEnabled ? "enabled" : "disabled";
        }
    }
}
