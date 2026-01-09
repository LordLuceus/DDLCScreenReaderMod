using HarmonyLib;
using RenpyParser;
using TMPro;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class DialoguePatches
    {
        // Track recent dialogue to prevent duplicates from constructor/setter calls
        private static string lastDialogueText = "";
        private static System.DateTime lastDialogueTime = System.DateTime.MinValue;
        private static readonly System.TimeSpan DialogueDuplicateWindow =
            System.TimeSpan.FromMilliseconds(100);

        private static bool IsDialogueDuplicate(string text)
        {
            var now = System.DateTime.Now;

            // Normalize text for comparison (handle dash variations and minor formatting differences)
            string normalizedText = NormalizeTextForComparison(text);
            string normalizedLast = NormalizeTextForComparison(lastDialogueText);

            bool isDuplicate =
                normalizedText == normalizedLast
                && (now - lastDialogueTime) < DialogueDuplicateWindow;

            if (!isDuplicate)
            {
                lastDialogueText = text;
                lastDialogueTime = now;
            }

            return isDuplicate;
        }

        private static string NormalizeTextForComparison(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            // Normalize various dash representations to detect duplicates
            return text.Replace("—", "--") // em dash to double dash
                .Replace("–", "-") // en dash to single dash
                .Trim();
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

        [HarmonyPatch(
            typeof(DialogueLine),
            MethodType.Constructor,
            new System.Type[]
            {
                typeof(string),
                typeof(string),
                typeof(int),
                typeof(string),
                typeof(string),
                typeof(bool),
                typeof(bool),
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(bool),
                typeof(bool),
                typeof(int),
                typeof(System.Collections.Generic.List<System.Tuple<int, float>>),
                typeof(string),
            }
        )]
        [HarmonyPostfix]
        public static void DialogueLine_Constructor_Postfix(
            DialogueLine __instance,
            string label,
            string text
        )
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    // Check for duplicate dialogue before processing
                    if (IsDialogueDuplicate(text))
                    {
                        return;
                    }

                    string speakerCode = string.IsNullOrWhiteSpace(__instance.Tag)
                        ? ""
                        : __instance.Tag;

                    // Filter out unwanted text
                    if (SpeakerMapping.ShouldFilterText(text, speakerCode))
                    {
                        ScreenReaderMod.Logger?.Msg(
                            $"Filtered out text: Speaker: '{speakerCode}', Text: '{text}'"
                        );
                        return;
                    }

                    string speakerName = SpeakerMapping.GetSpeakerName(speakerCode);

                    // Log for debugging
                    ScreenReaderMod.Logger?.Msg(
                        $"Dialogue line created - Code: '{speakerCode}' -> Name: '{speakerName}', Text: '{text}'"
                    );

                    // If we have a speaker name, always treat as dialogue
                    TextType textType = !string.IsNullOrWhiteSpace(speakerName)
                        ? TextType.Dialogue
                        : (
                            TextHelper.IsNarrativeText(text) ? TextType.Narrator : TextType.Dialogue
                        );
                    ClipboardUtils.OutputGameText(speakerName, text, textType);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DialogueLine_Constructor_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(DialogueLine), "Text", MethodType.Setter)]
        [HarmonyPostfix]
        public static void DialogueLine_Text_Setter_Postfix(DialogueLine __instance, string value)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Check for duplicate dialogue before processing
                    if (IsDialogueDuplicate(value))
                    {
                        return;
                    }

                    string speakerCode = string.IsNullOrWhiteSpace(__instance.Tag)
                        ? ""
                        : __instance.Tag;

                    // Filter out unwanted text
                    if (SpeakerMapping.ShouldFilterText(value, speakerCode))
                    {
                        ScreenReaderMod.Logger?.Msg(
                            $"Filtered out text update: Speaker: '{speakerCode}', Text: '{value}'"
                        );
                        return;
                    }

                    string speakerName = SpeakerMapping.GetSpeakerName(speakerCode);

                    // Log for debugging
                    ScreenReaderMod.Logger?.Msg(
                        $"Dialogue text set - Code: '{speakerCode}' -> Name: '{speakerName}', Text: '{value}'"
                    );

                    // If we have a speaker name, always treat as dialogue
                    TextType textType = !string.IsNullOrWhiteSpace(speakerName)
                        ? TextType.Dialogue
                        : (
                            TextHelper.IsNarrativeText(value)
                                ? TextType.Narrator
                                : TextType.Dialogue
                        );
                    ClipboardUtils.OutputGameText(speakerName, value, textType);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DialogueLine_Text_Setter_Postfix: {ex.Message}"
                );
            }
        }
    }
}
