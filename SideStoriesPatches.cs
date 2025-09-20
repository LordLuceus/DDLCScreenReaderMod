using HarmonyLib;
using RenpyLauncher;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class SideStoriesPatches
    {
        // Announce the side stories menu descriptive text
        [HarmonyPatch(typeof(SideStoriesApp), "SetupLocalization")]
        [HarmonyPostfix]
        public static void SideStoriesApp_SetupLocalization_Postfix(SideStoriesApp __instance)
        {
            try
            {
                if (__instance?.sideStoreText != null && !string.IsNullOrWhiteSpace(__instance.sideStoreText.text))
                {
                    ScreenReaderMod.Logger?.Msg($"Side stories menu text: {__instance.sideStoreText.text}");
                    ClipboardUtils.OutputGameText("", __instance.sideStoreText.text, TextType.Menu);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in SideStoriesApp_SetupLocalization_Postfix: {ex.Message}"
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

        private static string GetFriendlyStoryName(string renpyLabel)
        {
            // Log the renpy label for debugging
            if (!string.IsNullOrWhiteSpace(renpyLabel))
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