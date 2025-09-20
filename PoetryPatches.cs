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
                if (ModConfig.Instance.EnablePoetryGameAnnouncements)
                {
                    var word = __instance.Word;
                    string wordText = word.Word.word;

                    if (!string.IsNullOrWhiteSpace(wordText))
                    {
                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg($"Poetry word selected: '{wordText}'");

                        ClipboardUtils.OutputGameText("", wordText, TextType.PoetryGame);
                    }
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
                if (ModConfig.Instance.EnablePoetryGameAnnouncements)
                {
                    var word = __instance.Word;
                    string wordText = word.Word.word;

                    if (!string.IsNullOrWhiteSpace(wordText))
                    {
                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg($"Poetry word hovered: '{wordText}'");

                        ClipboardUtils.OutputGameText("", wordText, TextType.PoetryGame);
                    }
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
                if (ModConfig.Instance.EnablePoetryGameAnnouncements)
                {
                    ClipboardUtils.OutputGameText(
                        "",
                        "Poetry minigame started. Navigate with arrow keys and press Enter to select words.",
                        TextType.SystemMessage
                    );
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in PoetryGameScreen_BeginPoetryGame_Postfix: {ex.Message}"
                );
            }
        }
    }
}
