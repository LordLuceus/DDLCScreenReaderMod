using DDLC.Screens.PoetryGameScreen;
using HarmonyLib;
using UnityEngine.EventSystems;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class PoetryPatches
    {
        [HarmonyPatch(typeof(WordSelect), "OnSelect")]
        [HarmonyPostfix]
        public static void WordSelect_OnSelect_Postfix(
            WordSelect __instance,
            BaseEventData eventData
        )
        {
            try
            {
                var word = __instance.Word;
                string wordText = word.Word.word;

                if (!string.IsNullOrWhiteSpace(wordText))
                {
                    ScreenReaderMod.Logger?.Msg($"Poetry word selected: '{wordText}'");

                    ClipboardUtils.OutputGameText("", wordText, TextType.PoetryGame);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in WordSelect_OnSelect_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(WordSelect), "OnPointerEnter")]
        [HarmonyPostfix]
        public static void WordSelect_OnPointerEnter_Postfix(
            WordSelect __instance,
            PointerEventData eventData
        )
        {
            try
            {
                var word = __instance.Word;
                string wordText = word.Word.word;

                if (!string.IsNullOrWhiteSpace(wordText))
                {
                    ScreenReaderMod.Logger?.Msg($"Poetry word hovered: '{wordText}'");

                    ClipboardUtils.OutputGameText("", wordText, TextType.PoetryGame);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in WordSelect_OnPointerEnter_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(PoetryGameScreen), "BeginPoetryGame")]
        [HarmonyPostfix]
        public static void PoetryGameScreen_BeginPoetryGame_Postfix(PoetryGameScreen __instance)
        {
            try
            {
                ClipboardUtils.OutputGameText(
                    "",
                    "Poetry minigame started. Navigate with arrow keys and press Enter to select words.",
                    TextType.SystemMessage
                );
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in PoetryGameScreen_BeginPoetryGame_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(PoetryGameScreen), "OnWordSelected")]
        [HarmonyPrefix]
        public static void PoetryGameScreen_OnWordSelected_Prefix(
            PoetryGameScreen __instance,
            WordSelect selectedWord
        )
        {
            try
            {
                var word = selectedWord.Word;
                string wordText = word.Word.word;

                // Get current progress before it's incremented
                int currentProgress = (int)
                    __instance
                        .GetType()
                        .GetField(
                            "Progress",
                            System.Reflection.BindingFlags.NonPublic
                                | System.Reflection.BindingFlags.Instance
                        )
                        ?.GetValue(__instance);

                // Get character preferences for this word
                string characterReactions = GetCharacterReactions(word.Word);

                string message = "";

                if (!string.IsNullOrWhiteSpace(characterReactions))
                {
                    message = characterReactions;
                }

                message += (message.Length > 0 ? ". " : "") + $"Progress: {currentProgress}/20";

                ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in PoetryGameScreen_OnWordSelected_Prefix: {ex.Message}"
                );
            }
        }

        private static string GetCharacterReactions(
            RenPyParser.VGPrompter.DataHolders.PoetryWord poetryWord
        )
        {
            var reactions = new System.Collections.Generic.List<string>();

            // Check each character's favor level (3+ seems to be the threshold for reactions based on the original code)
            if (poetryWord.sayouriFavour >= 3)
                reactions.Add("Sayori likes this");
            if (poetryWord.natsukiFavour >= 3)
                reactions.Add("Natsuki likes this");
            if (poetryWord.yuriFavour >= 3)
                reactions.Add("Yuri likes this");

            if (reactions.Count == 0)
                return "";

            if (reactions.Count == 1)
                return reactions[0];

            if (reactions.Count == 2)
                return $"{reactions[0]} and {reactions[1]}";

            // All three like it
            return $"{reactions[0]}, {reactions[1]}, and {reactions[2]}";
        }
    }
}
