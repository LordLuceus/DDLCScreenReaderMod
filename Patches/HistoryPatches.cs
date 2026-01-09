using System;
using System.Collections.Generic;
using System.Linq;
using DDLC;
using HarmonyLib;
using RenpyParser;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class HistoryPatches
    {
        // Navigation state
        private static int currentHistoryIndex = -1;
        private static List<RenpyHistoryEntry> historyEntries = null;
        private static RenpyHistoryMenuUI activeHistoryScreen = null;

        /// <summary>
        /// Returns true only if the history screen is active AND has focus
        /// (i.e., the scrollbar is selected, not the navigation menu).
        /// </summary>
        public static bool IsHistoryScreenActive
        {
            get
            {
                if (activeHistoryScreen == null || historyEntries == null)
                    return false;

                // Check if the history scrollbar is currently selected
                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null)
                    return false;

                // The history screen is active only if its scrollbar has focus
                return activeHistoryScreen.HistoryScrollbar != null
                    && currentSelected == activeHistoryScreen.HistoryScrollbar.gameObject;
            }
        }

        [HarmonyPatch(typeof(RenpyHistoryMenuUI), "OnShow")]
        [HarmonyPostfix]
        public static void RenpyHistoryMenuUI_OnShow_Postfix(RenpyHistoryMenuUI __instance)
        {
            try
            {
                activeHistoryScreen = __instance;

                // Convert queue to list for indexed access
                historyEntries = Renpy.HistoryManager.HistoryLines.ToList();

                // Start at the most recent entry (end of list)
                currentHistoryIndex = historyEntries.Count - 1;

                if (historyEntries.Count == 0)
                {
                    ScreenReaderMod.Logger?.Msg("History screen opened - empty");
                    ClipboardUtils.OutputGameText("", "History is empty", TextType.Menu);
                }
                else
                {
                    string openMessage =
                        $"History, {historyEntries.Count} entries. Use up and down arrows to navigate.";
                    ScreenReaderMod.Logger?.Msg(
                        $"History screen opened with {historyEntries.Count} entries"
                    );
                    ClipboardUtils.OutputGameText("", openMessage, TextType.Menu);

                    // Announce the current (most recent) entry
                    AnnounceCurrentEntry();
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyHistoryMenuUI_OnShow_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(RenpyHistoryMenuUI), "OnHide")]
        [HarmonyPostfix]
        public static void RenpyHistoryMenuUI_OnHide_Postfix(RenpyHistoryMenuUI __instance)
        {
            try
            {
                activeHistoryScreen = null;
                currentHistoryIndex = -1;
                historyEntries = null;

                ScreenReaderMod.Logger?.Msg("History screen closed");
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyHistoryMenuUI_OnHide_Postfix: {ex.Message}"
                );
            }
        }

        public static void NavigateToPrevious()
        {
            if (!IsHistoryScreenActive || historyEntries == null || historyEntries.Count == 0)
                return;

            // Up arrow = older entry = lower index
            if (currentHistoryIndex > 0)
            {
                currentHistoryIndex--;
                AnnounceCurrentEntry();
            }
            else
            {
                ClipboardUtils.OutputGameText("", "Beginning of history", TextType.Menu);
            }
        }

        public static void NavigateToNext()
        {
            if (!IsHistoryScreenActive || historyEntries == null || historyEntries.Count == 0)
                return;

            // Down arrow = newer entry = higher index
            if (currentHistoryIndex < historyEntries.Count - 1)
            {
                currentHistoryIndex++;
                AnnounceCurrentEntry();
            }
            else
            {
                ClipboardUtils.OutputGameText("", "End of history", TextType.Menu);
            }
        }

        private static void AnnounceCurrentEntry()
        {
            if (
                historyEntries == null
                || currentHistoryIndex < 0
                || currentHistoryIndex >= historyEntries.Count
            )
                return;

            var entry = historyEntries[currentHistoryIndex];
            string speaker = GetSpeakerName(entry);
            string text = GetEntryText(entry);

            if (string.IsNullOrEmpty(speaker))
            {
                // Narrator text - no prefix
                ScreenReaderMod.Logger?.Msg($"History: [Narrator] {text}");
                ClipboardUtils.OutputGameText("", text, TextType.Narrator);
            }
            else
            {
                // Dialogue with speaker
                ScreenReaderMod.Logger?.Msg($"History: {speaker}: {text}");
                ClipboardUtils.OutputGameText(speaker, text, TextType.Dialogue);
            }
        }

        private static string GetSpeakerName(RenpyHistoryEntry entry)
        {
            return SpeakerMapping.GetLocalizedSpeakerName(entry.who);
        }

        private static string GetEntryText(RenpyHistoryEntry entry)
        {
            try
            {
                string rawText;

                if (entry.what == null)
                {
                    // Use whatID for localization lookup
                    rawText = Renpy.Text.GetLocalisedString(
                        entry.whatID,
                        LocalisedStringPostProcessing.None,
                        entry.label
                    );
                }
                else
                {
                    // Use what string
                    rawText = Renpy.Text.GetLocalisedString(
                        entry.what,
                        LocalisedStringPostProcessing.None,
                        entry.label
                    );
                }

                // Interpolate variables (player name, etc.)
                string interpolated = Renpy.ContextControl.InterpolateText(rawText);

                // Clean the text using the mod's text cleaner
                return TextHelper.CleanText(interpolated);
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Warning($"Error getting entry text: {ex.Message}");
                return entry.what ?? "[Error reading text]";
            }
        }
    }
}
