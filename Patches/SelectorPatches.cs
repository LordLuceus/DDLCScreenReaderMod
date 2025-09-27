using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class SelectorPatches
    {
        // Track current focus to prevent duplicate announcements
        private static GameObject lastFocusedSelector = null;
        private static string lastAnnouncedValue = "";
        private static DateTime lastAnnouncementTime = DateTime.MinValue;
        private static readonly TimeSpan AnnouncementCooldown = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Get friendly name for a selector based on its provider type
        /// </summary>
        private static string GetSelectorName(UISelector selector)
        {
            if (selector?.itemsProvider == null)
                return "Setting";

            string typeName = selector.itemsProvider.GetType().Name;

            // Extract name from provider type (e.g., "ResolutionSelectorProvider" -> "Resolution")
            if (typeName.EndsWith("SelectorProvider"))
            {
                typeName = typeName.Substring(0, typeName.Length - "SelectorProvider".Length);
            }

            // Convert to friendly names
            return typeName switch
            {
                "Resolution" => "Resolution",
                "VSync" => "VSync",
                "Windowed" => "Display Mode",
                _ => typeName, // Fallback to type name
            };
        }

        /// <summary>
        /// Get current value from UISelector using reflection
        /// </summary>
        private static string GetCurrentValue(UISelector selector)
        {
            try
            {
                // Use reflection to access private fields
                var itemsField = AccessTools.Field(typeof(UISelector), "items");
                var currentItemField = AccessTools.Field(typeof(UISelector), "currentItem");

                if (itemsField == null || currentItemField == null)
                    return "Unknown";

                var items = itemsField.GetValue(selector) as SelectorItem[];
                var currentItem = (int)currentItemField.GetValue(selector);

                if (items != null && currentItem >= 0 && currentItem < items.Length)
                {
                    return items[currentItem].GetLocalizedString();
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting current value: {ex.Message}");
            }

            return "Unknown";
        }

        /// <summary>
        /// Check if we should announce (prevents spam during rapid navigation)
        /// </summary>
        private static bool ShouldAnnounce(string value)
        {
            var now = DateTime.Now;
            if (value == lastAnnouncedValue && (now - lastAnnouncementTime) < AnnouncementCooldown)
            {
                return false;
            }

            lastAnnouncedValue = value;
            lastAnnouncementTime = now;
            return true;
        }

        /// <summary>
        /// Announce UISelector focus with current value
        /// </summary>
        [HarmonyPatch(
            typeof(UISelector),
            "EnableSelectable",
            new Type[] { typeof(bool), typeof(bool) }
        )]
        [HarmonyPostfix]
        public static void UISelector_EnableSelectable_Postfix(
            UISelector __instance,
            bool enable,
            bool active
        )
        {
            try
            {
                // Only announce when becoming active (focused)
                if (!active || !enable)
                    return;

                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null)
                    return;

                // Check if this selector is currently selected
                var selectableComponent = __instance.GetSelectable();
                if (selectableComponent?.gameObject != currentSelected)
                    return;

                // Prevent duplicate announcements
                if (lastFocusedSelector == currentSelected)
                    return;
                lastFocusedSelector = currentSelected;

                string selectorName = GetSelectorName(__instance);
                string currentValue = GetCurrentValue(__instance);
                string message = $"{selectorName}, {currentValue}";

                ScreenReaderMod.Logger?.Msg($"UISelector focused: {message}");
                ClipboardUtils.OutputGameText("", message, TextType.Settings);
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in UISelector_EnableSelectable_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce UISelector value changes during navigation
        /// </summary>
        [HarmonyPatch(typeof(UISelector), "SetCurrentItem")]
        [HarmonyPostfix]
        public static void UISelector_SetCurrentItem_Postfix(UISelector __instance, int value)
        {
            try
            {
                // Only announce if this selector is currently focused
                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null)
                    return;

                var selectableComponent = __instance.GetSelectable();
                if (selectableComponent?.gameObject != currentSelected)
                    return;

                // Get the new value
                string newValue = GetCurrentValue(__instance);

                if (ShouldAnnounce(newValue))
                {
                    ScreenReaderMod.Logger?.Msg($"UISelector value changed: {newValue}");
                    ClipboardUtils.OutputGameText("", newValue, TextType.Settings);
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in UISelector_SetCurrentItem_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce UISelector value application
        /// </summary>
        [HarmonyPatch(typeof(UISelector), "ActivateCurrentItem")]
        [HarmonyPostfix]
        public static void UISelector_ActivateCurrentItem_Postfix(UISelector __instance)
        {
            try
            {
                string selectorName = GetSelectorName(__instance);
                string currentValue = GetCurrentValue(__instance);
                string message = $"{selectorName} changed to {currentValue}";

                ScreenReaderMod.Logger?.Msg($"UISelector applied: {message}");
                ClipboardUtils.OutputGameText("", message, TextType.Settings);
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in UISelector_ActivateCurrentItem_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce LanguageSelector navigation
        /// </summary>
        [HarmonyPatch(typeof(LanguageSelector), "NextLanguage")]
        [HarmonyPostfix]
        public static void LanguageSelector_NextLanguage_Postfix(LanguageSelector __instance)
        {
            try
            {
                // Only announce if this selector is currently focused
                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null || currentSelected != __instance.applyButton)
                    return;

                // Get current language name from the display text
                string languageName = __instance.languageText?.text ?? "Unknown";

                if (ShouldAnnounce(languageName))
                {
                    ScreenReaderMod.Logger?.Msg($"Language changed to: {languageName}");
                    ClipboardUtils.OutputGameText("", languageName, TextType.Settings);
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in LanguageSelector_NextLanguage_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce LanguageSelector navigation (previous)
        /// </summary>
        [HarmonyPatch(typeof(LanguageSelector), "PreviousLanguage")]
        [HarmonyPostfix]
        public static void LanguageSelector_PreviousLanguage_Postfix(LanguageSelector __instance)
        {
            try
            {
                // Only announce if this selector is currently focused
                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null || currentSelected != __instance.applyButton)
                    return;

                // Get current language name from the display text
                string languageName = __instance.languageText?.text ?? "Unknown";

                if (ShouldAnnounce(languageName))
                {
                    ScreenReaderMod.Logger?.Msg($"Language changed to: {languageName}");
                    ClipboardUtils.OutputGameText("", languageName, TextType.Settings);
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in LanguageSelector_PreviousLanguage_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce LanguageSelector application
        /// </summary>
        [HarmonyPatch(typeof(LanguageSelector), "ApplyLanguageChange")]
        [HarmonyPostfix]
        public static void LanguageSelector_ApplyLanguageChange_Postfix(LanguageSelector __instance)
        {
            try
            {
                string languageName = __instance.languageText?.text ?? "Unknown";
                string message = $"Language changed to {languageName}";

                ScreenReaderMod.Logger?.Msg($"Language applied: {message}");
                ClipboardUtils.OutputGameText("", message, TextType.Settings);
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in LanguageSelector_ApplyLanguageChange_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Reset focus tracking when focus changes away from selectors
        /// </summary>
        [HarmonyPatch(
            typeof(EventSystem),
            "SetSelectedGameObject",
            new Type[] { typeof(GameObject) }
        )]
        [HarmonyPostfix]
        public static void EventSystem_SetSelectedGameObject_Postfix(
            EventSystem __instance,
            GameObject selected
        )
        {
            try
            {
                // Reset tracking when focus changes
                if (selected != lastFocusedSelector)
                {
                    lastFocusedSelector = null;
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in EventSystem_SetSelectedGameObject_Postfix: {ex.Message}"
                );
            }
        }
    }
}
