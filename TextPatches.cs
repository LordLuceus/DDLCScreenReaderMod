using HarmonyLib;
using RenpyParser;
using TMPro;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class TextPatches
    {
        // Track recent text to prevent duplicates
        private static string lastTextValue = "";
        private static System.DateTime lastTextTime = System.DateTime.MinValue;
        private static readonly System.TimeSpan TextDuplicateWindow =
            System.TimeSpan.FromMilliseconds(100);

        private static bool IsTextDuplicate(string text)
        {
            var now = System.DateTime.Now;

            bool isDuplicate =
                text == lastTextValue
                && (now - lastTextTime) < TextDuplicateWindow;

            if (!isDuplicate)
            {
                lastTextValue = text;
                lastTextTime = now;
            }

            return isDuplicate;
        }

        [HarmonyPatch(typeof(RenpyStandardProxyLib.Text), "Immediate")]
        [HarmonyPostfix]
        public static void RenpyStandardProxyLibText_Immediate_Postfix(
            RenpyStandardProxyLib.Text __instance,
            GameObject gameObject
        )
        {
            try
            {
                // Get the TextMeshProUGUI component that was just set up
                var textComponent = gameObject.GetComponentInChildren<TextMeshProUGUI>();

                if (textComponent == null || string.IsNullOrWhiteSpace(textComponent.text))
                {
                    return;
                }

                string rawText = textComponent.text;

                // Check for duplicate text before processing
                if (IsTextDuplicate(rawText))
                {
                    return;
                }

                // Clean the text using the mod's existing text processor
                string cleanedText = TextProcessor.CleanText(rawText);

                if (string.IsNullOrWhiteSpace(cleanedText))
                {
                    return;
                }

                // Log for debugging
                ScreenReaderMod.Logger?.Msg(
                    $"Intercepted text from RenpyStandardProxyLib.Text: '{cleanedText}'"
                );

                // Output as narrator text since this appears to be general text display
                ClipboardUtils.OutputGameText("", cleanedText, TextType.Narrator);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyStandardProxyLibText_Immediate_Postfix: {ex.Message}"
                );
            }
        }
    }
}