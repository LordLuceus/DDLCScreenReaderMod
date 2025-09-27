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

            // Get current playthrough to determine which characters are available
            int playthrough = GetCurrentPlaythrough();

            if (playthrough == 0)
            {
                // Act 1: Show all character reactions based on individual favor levels
                if (poetryWord.sayouriFavour >= 3)
                    reactions.Add("Sayori likes this");
                if (poetryWord.natsukiFavour >= 3)
                    reactions.Add("Natsuki likes this");
                if (poetryWord.yuriFavour >= 3)
                    reactions.Add("Yuri likes this");
            }
            else if (playthrough >= 1 && playthrough <= 2)
            {
                // Act 2: Only show Natsuki vs Yuri reactions (Sayori is gone 😭)
                // Based on the game logic: if natsukiFavour > yuriFavour, show Natsuki, else show Yuri
                if (poetryWord.natsukiFavour > poetryWord.yuriFavour)
                {
                    reactions.Add("Natsuki likes this");
                }
                else
                {
                    reactions.Add("Yuri likes this");
                }
            }
            else if (playthrough == 3)
            {
                // Act 3: Only Monika exists, but she doesn't have favor values in the original poetry word data
                // The game doesn't show reactions in Act 3, so we'll return empty
                return "";
            }
            else if (playthrough == 4)
            {
                // Act 4: Similar to Act 1 but without Monika - though this might not have poetry games
                if (poetryWord.sayouriFavour >= 3)
                    reactions.Add("Sayori likes this");
                if (poetryWord.natsukiFavour >= 3)
                    reactions.Add("Natsuki likes this");
                if (poetryWord.yuriFavour >= 3)
                    reactions.Add("Yuri likes this");
            }

            if (reactions.Count == 0)
                return "";

            if (reactions.Count == 1)
                return reactions[0];

            if (reactions.Count == 2)
                return $"{reactions[0]} and {reactions[1]}";

            // All three like it (only possible in Acts 1 and 4)
            return $"{reactions[0]}, {reactions[1]}, and {reactions[2]}";
        }

        private static int GetCurrentPlaythrough()
        {
            try
            {
                return (int)
                    System.Math.Floor(
                        Renpy.CurrentContext.GetVariableFloat("persistent.playthrough")
                    );
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting playthrough: {ex.Message}");
                return 0; // Default to playthrough 0 if we can't determine it
            }
        }
    }
}
