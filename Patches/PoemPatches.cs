using HarmonyLib;
using RenpyParser;
using RenPyParser.Screens.PoemScreen;
using UnityEngine;

namespace DDLCPlusAccess
{
    [HarmonyPatch]
    public static class PoemPatches
    {
        // Track recent poems to prevent duplicates
        private static string lastPoemText = "";
        private static System.DateTime lastPoemTime = System.DateTime.MinValue;
        private static readonly System.TimeSpan PoemDuplicateWindow =
            System.TimeSpan.FromMilliseconds(500);

        private static bool IsPoemDuplicate(string text)
        {
            var now = System.DateTime.Now;
            if (text == lastPoemText && (now - lastPoemTime) < PoemDuplicateWindow)
            {
                return true;
            }

            lastPoemText = text;
            lastPoemTime = now;
            return false;
        }

        [HarmonyPatch(typeof(RenpyPoemUI), "OnRefreshStyle")]
        [HarmonyPostfix]
        public static void OnRefreshStylePostfix(RenpyPoemUI __instance)
        {
            try
            {
                // Access the private poem field using reflection
                var poemField = typeof(RenpyPoemUI).GetField(
                    "m_Poem",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );

                if (poemField == null)
                {
                    ScreenReaderMod.Logger?.Warning("Could not access poem field in RenpyPoemUI");
                    return;
                }

                var poem = poemField.GetValue(__instance) as Poem;
                if (poem == null)
                {
                    ScreenReaderMod.Logger?.Msg("No poem data found");
                    return;
                }

                // Build the poem text for screen reader
                string poemText = BuildPoemText(poem);

                if (string.IsNullOrWhiteSpace(poemText) || IsPoemDuplicate(poemText))
                    return;

                // Skip normal text processing for poems to preserve formatting
                ClipboardUtils.OutputPoemText(poemText);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in poem patch: {ex.Message}");
            }
        }

        private static string BuildPoemText(Poem poem)
        {
            string result = "";

            // Add title if present
            if (!string.IsNullOrWhiteSpace(poem.title))
            {
                string title = poem.title;

                // Handle localization for non-yuri_3 poems
                if (!poem.yuri_3)
                {
                    try
                    {
                        title = Renpy.Text.GetLocalisedString(poem.title);
                    }
                    catch
                    {
                        // Use original title if localization fails
                        title = poem.title;
                    }
                }

                result += title + "\n\n";
            }

            // Add poem content
            if (!string.IsNullOrWhiteSpace(poem.text))
            {
                string content = poem.text;

                // Handle localization for non-yuri_3 poems
                if (!poem.yuri_3)
                {
                    try
                    {
                        // Don't pre-process, let localization handle it
                        content = Renpy.Text.GetLocalisedString(
                            content,
                            LocalisedStringPostProcessing.ReplaceLineBreaks,
                            "persistent",
                            ignoreSymbolsForEnglish: true
                        );
                    }
                    catch
                    {
                        // Use original content if localization fails, convert line breaks manually
                        content = content.Replace("\\n", "\n");
                    }
                }
                else
                {
                    // For yuri_3 poems, manually convert line breaks
                    content = content.Replace("\\n", "\n");
                }

                // Clean Ren'Py tags while preserving line breaks, and normalize line breaks
                content = CleanPoemText(content);
                result += content;
            }

            return result;
        }

        private static string CleanPoemText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string cleaned = input;

            // Remove Ren'Py formatting tags but preserve line breaks
            cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\{[^}]*\}", "");
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\[color=[^]]*\]|\[/color\]",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\[size=[^]]*\]|\[/size\]",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\[b\]|\[/b\]",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\[i\]|\[/i\]",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );
            cleaned = System.Text.RegularExpressions.Regex.Replace(
                cleaned,
                @"\[u\]|\[/u\]",
                "",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Unescape special characters, using single line breaks
            cleaned = cleaned.Replace("\\n", "\n");
            cleaned = cleaned.Replace("\\r", "\r");
            cleaned = cleaned.Replace("\\t", "\t");
            cleaned = cleaned.Replace("\\\"", "\"");
            cleaned = cleaned.Replace("\\'", "'");
            cleaned = cleaned.Replace("\\\\", "\\");

            // Normalize all types of line break combinations to single line breaks
            cleaned = cleaned.Replace("\r\n", "\n").Replace("\r", "\n");

            return cleaned;
        }
    }
}
