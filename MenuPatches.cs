using HarmonyLib;
using RenpyParser;
using UnityEngine;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class MenuPatches
    {
        [HarmonyPatch(typeof(RenpyChoiceMenuUI), "ShowChoiceMenu")]
        [HarmonyPostfix]
        public static void RenpyChoiceMenuUI_ShowChoiceMenu_Postfix(
            RenpyChoiceMenuUI __instance,
            System.Collections.Generic.List<RenpyChoiceEntryUI> entries,
            int activeCount
        )
        {
            try
            {
                ScreenReaderMod.Logger?.Msg($"Choice menu shown with {activeCount} choices");

                ClipboardUtils.OutputGameText(
                    "",
                    $"Choose from {activeCount} option{(activeCount > 1 ? "s" : "")}",
                    TextType.Menu
                );
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyChoiceMenuUI_ShowChoiceMenu_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyMainMenuUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyMainMenuUI_OnShow_Postfix(RenpyMainMenuUI __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Main menu shown");
                ClipboardUtils.OutputGameText("", "Main menu", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyMainMenuUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyPreferencesMenuUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyPreferencesMenuUI_OnShow_Postfix(RenpyPreferencesMenuUI __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Preferences menu shown");
                ClipboardUtils.OutputGameText("", "Preferences menu", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyPreferencesMenuUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyHistoryMenuUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyHistoryMenuUI_OnShow_Postfix(RenpyHistoryMenuUI __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("History menu shown");
                ClipboardUtils.OutputGameText("", "History menu", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyHistoryMenuUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpySaveLoadScreen), "OnShow")]
        [HarmonyPostfix]
        public static void RenpySaveLoadScreen_OnShow_Postfix(RenpySaveLoadScreen __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Save/Load screen shown");
                ClipboardUtils.OutputGameText("", "Save and load menu", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpySaveLoadScreen_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyConfirmScreenUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyConfirmScreenUI_OnShow_Postfix(RenpyConfirmScreenUI __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Confirmation dialog shown");
                ClipboardUtils.OutputGameText("", "Confirmation dialog", TextType.SystemMessage);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyConfirmScreenUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyNameInputScreenUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyNameInputScreenUI_OnShow_Postfix(RenpyNameInputScreenUI __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Name input screen shown");
                ClipboardUtils.OutputGameText("", "Name input screen", TextType.SystemMessage);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyNameInputScreenUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyButtonUI), "OnButtonFocussed")]
        [HarmonyPostfix]
        public static void RenpyButtonUI_OnButtonFocussed_Postfix(RenpyButtonUI __instance)
        {
            try
            {
                if (
                    __instance.textObject != null
                    && !string.IsNullOrWhiteSpace(__instance.textObject.text)
                )
                {
                    string buttonText = __instance.textObject.text;
                    ScreenReaderMod.Logger?.Msg($"Button focused: {buttonText}");
                    ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyButtonUI_OnButtonFocussed_Postfix: {ex.Message}"
                );
            }
        }
    }
}
