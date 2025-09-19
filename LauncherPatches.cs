using HarmonyLib;
using RenpyLauncher;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class LauncherPatches
    {
        // Rate limiting for confirmation dialog to prevent spam
        private static string lastConfirmationButtonText = "";
        private static UnityEngine.GameObject lastConfirmationButtonObject = null;

        // Patch button selection for navigation announcements
        [HarmonyPatch(
            typeof(DesktopApp),
            "SelectButton",
            new System.Type[] { typeof(StartMenuButton) }
        )]
        [HarmonyPostfix]
        public static void DesktopApp_SelectButton_Postfix(DesktopApp __instance, StartMenuButton b)
        {
            try
            {
                if (b != null && b.Text != null && !string.IsNullOrWhiteSpace(b.Text.text))
                {
                    string buttonText = b.Text.text;
                    if (ModConfig.Instance.EnableVerboseLogging)
                        ScreenReaderMod.Logger?.Msg($"Launcher button selected: {buttonText}");
                    ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopApp_SelectButton_Postfix: {ex.Message}"
                );
            }
        }

        // Side Stories app accessibility
        [HarmonyPatch(typeof(SideStoriesApp), "OnItemSelected")]
        [HarmonyPostfix]
        public static void SideStoriesApp_OnItemSelected_Postfix(
            SideStoriesApp __instance,
            Button selected
        )
        {
            try
            {
                if (selected != null)
                {
                    // Detect which part this is by finding which button it is in the SideStoryInfoComponent
                    string storyTitle = GetStoryTitleWithPart(__instance, selected);

                    if (!string.IsNullOrWhiteSpace(storyTitle))
                    {
                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg($"Side story selected: {storyTitle}");
                        ClipboardUtils.OutputGameText("", storyTitle, TextType.Menu);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in SideStoriesApp_OnItemSelected_Postfix: {ex.Message}"
                );
            }
        }

        private static string GetStoryTitleWithPart(SideStoriesApp sideStoriesApp, Button selected)
        {
            if (sideStoriesApp == null || selected == null)
                return null;

            try
            {
                // Access the SideStoryInfoComponents to determine which part this button is
                var sideStoryInfoComponents = sideStoriesApp.SideStoryInfoComponents;
                if (sideStoryInfoComponents == null)
                    return null;

                foreach (var component in sideStoryInfoComponents)
                {
                    if (component.Button1 == selected)
                    {
                        // This is Button1 - first part
                        string baseTitle = GetActualStoryTitle(selected);
                        if (!string.IsNullOrWhiteSpace(baseTitle))
                        {
                            return $"{baseTitle} Part 1";
                        }
                        else
                        {
                            return GetFriendlyStoryName(selected.name) + " Part 1";
                        }
                    }
                    else if (component.Button2 == selected)
                    {
                        // This is Button2 - second part
                        string baseTitle = GetActualStoryTitle(selected);
                        if (!string.IsNullOrWhiteSpace(baseTitle))
                        {
                            return $"{baseTitle} Part 2";
                        }
                        else
                        {
                            return GetFriendlyStoryName(selected.name) + " Part 2";
                        }
                    }
                }

                // Fallback - couldn't determine which button it is
                string fallbackTitle = GetActualStoryTitle(selected);
                if (!string.IsNullOrWhiteSpace(fallbackTitle))
                {
                    return fallbackTitle;
                }
                else if (!string.IsNullOrWhiteSpace(selected.name))
                {
                    return GetFriendlyStoryName(selected.name);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in GetStoryTitleWithPart: {ex.Message}");
            }

            return null;
        }

        private static string GetActualStoryTitle(Button button)
        {
            if (button == null)
                return null;

            try
            {
                // Search for TextMeshProUGUI components in the button and its children
                var textComponents = button.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var textComponent in textComponents)
                {
                    if (!string.IsNullOrWhiteSpace(textComponent.text))
                    {
                        // Skip if it's just the renpy label code or generic text
                        string lowerText = textComponent.text.ToLower();
                        if (
                            !lowerText.Contains("exclusive")
                            && !lowerText.StartsWith("s")
                            && !lowerText.Equals("part")
                            && textComponent.text.Length > 3
                            && !IsGenericUIText(textComponent.text)
                        )
                        {
                            return textComponent.text;
                        }
                    }
                }

                // Also try regular Text components
                var legacyTextComponents = button.GetComponentsInChildren<UnityEngine.UI.Text>(
                    true
                );
                foreach (var textComponent in legacyTextComponents)
                {
                    if (!string.IsNullOrWhiteSpace(textComponent.text))
                    {
                        string lowerText = textComponent.text.ToLower();
                        if (
                            !lowerText.Contains("exclusive")
                            && !lowerText.StartsWith("s")
                            && !lowerText.Equals("part")
                            && textComponent.text.Length > 3
                            && !IsGenericUIText(textComponent.text)
                        )
                        {
                            return textComponent.text;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error searching for story title: {ex.Message}");
            }

            return null;
        }

        private static bool IsGenericUIText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            string lower = text.ToLower().Trim();

            // Filter out common UI text that isn't story titles
            return lower == "part"
                || lower == "1"
                || lower == "2"
                || lower == "i"
                || lower == "ii"
                || lower.Length <= 2;
        }

        // Confirmation dialog accessibility
        [HarmonyPatch(typeof(DesktopConfirmationWindow), "ShowMessageDialog")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_ShowMessageDialog_Postfix(
            DesktopConfirmationWindow __instance,
            string message,
            string header
        )
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    ClipboardUtils.OutputGameText("", header, TextType.SystemMessage);
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_ShowMessageDialog_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(DesktopConfirmationWindow), "UpdateSelection")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_UpdateSelection_Postfix(
            DesktopConfirmationWindow __instance
        )
        {
            try
            {
                var currentSelected = UnityEngine
                    .EventSystems
                    .EventSystem
                    .current
                    ?.currentSelectedGameObject;
                if (currentSelected != null)
                {
                    string buttonText = "";
                    if (
                        currentSelected == __instance.ConfirmButton?.gameObject
                        && __instance.confirmText?.text != null
                    )
                    {
                        buttonText = __instance.confirmText.text;
                    }
                    else if (
                        currentSelected == __instance.CancelButton?.gameObject
                        && __instance.backText?.text != null
                    )
                    {
                        buttonText = __instance.backText.text;
                    }
                    else if (
                        currentSelected == __instance.OKButton?.gameObject
                        && __instance.okText?.text != null
                    )
                    {
                        buttonText = __instance.okText.text;
                    }

                    if (!string.IsNullOrWhiteSpace(buttonText))
                    {
                        // Only announce if this is a different button than last time
                        if (currentSelected != lastConfirmationButtonObject)
                        {
                            lastConfirmationButtonText = buttonText;
                            lastConfirmationButtonObject = currentSelected;

                            if (ModConfig.Instance.EnableVerboseLogging)
                                ScreenReaderMod.Logger?.Msg(
                                    $"Confirmation button focused: {buttonText}"
                                );
                            ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_UpdateSelection_Postfix: {ex.Message}"
                );
            }
        }

        private static string GetFriendlyStoryName(string renpyLabel)
        {
            // Log the renpy label for debugging
            if (ModConfig.Instance.EnableVerboseLogging && !string.IsNullOrWhiteSpace(renpyLabel))
            {
                ScreenReaderMod.Logger?.Msg($"GetFriendlyStoryName called with: '{renpyLabel}'");
            }

            // Convert renpy labels to friendly names with actual titles
            switch (renpyLabel?.ToLower())
            {
                case "sayori_exclusive1":
                case "se1":
                    return "Trust"; // First Sayori story
                case "sayori_exclusive2":
                case "se2":
                    return "Understanding"; // Second Sayori story
                case "natsuki_exclusive1":
                case "ne1":
                    return "Respect"; // First Natsuki story
                case "natsuki_exclusive2":
                case "ne2":
                    return "My Meadow"; // Second Natsuki story
                case "yuri_exclusive1":
                case "ye1":
                    return "Self-Love"; // First Yuri story
                case "yuri_exclusive2":
                case "ye2":
                    return "Balanced Breakfast"; // Second Yuri story
                case "sm_exclusive":
                    return "Thank You"; // Sayori & Monika
                case "sy_exclusive":
                    return "Sayori & Yuri";
                case "sn_exclusive":
                    return "Sayori & Natsuki";
                case "nm_exclusive":
                    return "Natsuki & Monika";
                case "ny_exclusive":
                    return "Natsuki & Yuri";
                case "ym_exclusive":
                    return "Yuri & Monika";
                case "testvm":
                    return "Test VM";
                case "ny1":
                case "ny4":
                    return "Self-Love";
                default:
                    // Log unknown labels for debugging
                    if (!string.IsNullOrWhiteSpace(renpyLabel))
                    {
                        ScreenReaderMod.Logger?.Msg($"Unknown side story label: '{renpyLabel}'");
                    }
                    return renpyLabel ?? "Unknown Story";
            }
        }
    }
}
