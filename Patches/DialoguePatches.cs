using HarmonyLib;
using RenpyParser;
using TMPro;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class DialoguePatches
    {
        /// <summary>
        /// Patch RenpyWindowManager.Say to capture dialogue with the correct localized speaker name.
        /// This matches exactly what the game displays.
        /// </summary>
        [HarmonyPatch(typeof(RenpyWindowManager), "Say")]
        [HarmonyPostfix]
        public static void RenpyWindowManager_Say_Postfix(
            string tag,
            string character,
            DialogueLine dialogueLine
        )
        {
            try
            {
                string text = dialogueLine.Text;
                if (string.IsNullOrWhiteSpace(text))
                    return;

                // Filter out unwanted text (developer commentary, etc.)
                string speakerCode = tag ?? "";
                if (SpeakerMapping.ShouldFilterText(text, speakerCode))
                {
                    ScreenReaderMod.Logger?.Msg(
                        $"Filtered out text: Speaker: '{speakerCode}', Text: '{text}'"
                    );
                    return;
                }

                // Get the localized speaker name using the game's exact logic
                string speakerName = SpeakerMapping.GetLocalizedSpeakerName(tag, character);

                // Clean the text
                string cleanedText = TextHelper.CleanText(text);

                // Log for debugging
                ScreenReaderMod.Logger?.Msg(
                    $"Say - Tag: '{tag}', Character: '{character}' -> Speaker: '{speakerName}', Text: '{cleanedText}'"
                );

                // Determine text type and output
                if (string.IsNullOrWhiteSpace(speakerName))
                {
                    // Narrator text
                    ClipboardUtils.OutputGameText("", cleanedText, TextType.Narrator);
                }
                else
                {
                    // Dialogue with speaker
                    ClipboardUtils.OutputGameText(speakerName, cleanedText, TextType.Dialogue);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyWindowManager_Say_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyDialogScreenUI), "SetData")]
        [HarmonyPostfix]
        public static void RenpyDialogScreenUI_SetData_Postfix(
            RenpyDialogScreenUI __instance,
            string messageStr
        )
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(messageStr))
                {
                    ClipboardUtils.OutputGameText("", messageStr, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyDialogScreenUI_SetData_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyDialogScreenUI), "OnRefreshStyle")]
        [HarmonyPostfix]
        public static void RenpyDialogScreenUI_OnRefreshStyle_Postfix(
            RenpyDialogScreenUI __instance
        )
        {
            try
            {
                if (__instance.text != null && !string.IsNullOrWhiteSpace(__instance.text.text))
                {
                    string dialogText = __instance.text.text;
                    ClipboardUtils.OutputGameText("", dialogText, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyDialogScreenUI_OnRefreshStyle_Postfix: {ex.Message}"
                );
            }
        }

        // Patch DDLC_Over18 component to announce age verification message
        [HarmonyPatch(typeof(DDLC_Over18), "RunAnimation")]
        [HarmonyPostfix]
        public static void DDLC_Over18_RunAnimation_Postfix(DDLC_Over18 __instance)
        {
            try
            {
                if (
                    __instance.textGUI != null
                    && !string.IsNullOrWhiteSpace(__instance.textGUI.text)
                )
                {
                    string ageVerificationText = __instance.textGUI.text;
                    ClipboardUtils.OutputGameText("", ageVerificationText, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DDLC_Over18_RunAnimation_Postfix: {ex.Message}"
                );
            }
        }
    }
}
