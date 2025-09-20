using System;
using MelonLoader;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    public static class ClipboardUtils
    {
        private static string lastText = "";
        private static DateTime lastUpdate = DateTime.MinValue;
        private static readonly TimeSpan MinUpdateInterval = TimeSpan.FromMilliseconds(500); // Increased to reduce duplicates

        private static string currentDialogueSpeaker = "";
        private static string currentDialogueText = "";
        private static TextType currentDialogueType = TextType.Dialogue;

        public static void Initialize()
        {
            ScreenReaderMod.Logger?.Msg("ClipboardUtils initialized");
        }

        public static void Cleanup()
        {
            ScreenReaderMod.Logger?.Msg("ClipboardUtils cleanup");
        }

        public static void RepeatCurrentDialogue()
        {
            if (!string.IsNullOrWhiteSpace(currentDialogueText))
            {
                string formattedText = FormatTextForScreenReader(
                    currentDialogueSpeaker,
                    currentDialogueText,
                    currentDialogueType
                );

                // Temporarily disable duplicate filtering for repeat
                string tempLastText = lastText;
                DateTime tempLastUpdate = lastUpdate;

                lastText = "";
                lastUpdate = DateTime.MinValue;

                if (SetClipboardText(formattedText, currentDialogueType))
                {
                    ScreenReaderMod.Logger?.Msg($"Repeated dialogue: '{formattedText}'");
                }

                // Restore duplicate filtering state
                lastText = tempLastText;
                lastUpdate = tempLastUpdate;
            }
            else
            {
                ScreenReaderMod.Logger?.Msg("No dialogue available to repeat");
            }
        }

        public static bool SetClipboardText(string text, TextType textType = TextType.Dialogue)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // SystemMessages should bypass rate limiting to ensure important announcements are never missed
            bool shouldBypassRateLimit = textType == TextType.SystemMessage;

            if (
                !shouldBypassRateLimit
                && text == lastText
                && DateTime.Now - lastUpdate < MinUpdateInterval
            )
                return false;

            try
            {
                GUIUtility.systemCopyBuffer = text;
                lastText = text;
                lastUpdate = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Failed to set clipboard text: {ex.Message}");
                return false;
            }
        }

        public static void OutputGameText(
            string speaker,
            string text,
            TextType textType = TextType.Dialogue
        )
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            string formattedText = FormatTextForScreenReader(speaker, text, textType);

            if (SetClipboardText(formattedText, textType))
            {
                // Store current dialogue for repeat functionality
                if (textType == TextType.Dialogue || textType == TextType.Narrator)
                {
                    currentDialogueSpeaker = speaker ?? "";
                    currentDialogueText = text;
                    currentDialogueType = textType;
                }

                // Always log clipboard output for debugging speaker names
                ScreenReaderMod.Logger?.Msg(
                    $"[{textType}] Clipboard: '{formattedText}' (Speaker: '{speaker}')"
                );
            }
        }

        public static void OutputPoemText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            if (SetClipboardText(text, TextType.Poem))
            {
                // Store current dialogue for repeat functionality
                currentDialogueSpeaker = "";
                currentDialogueText = text;
                currentDialogueType = TextType.Poem;

                ScreenReaderMod.Logger?.Msg($"[Poem] Clipboard: '{text}'");
            }
        }

        private static string FormatTextForScreenReader(
            string speaker,
            string text,
            TextType textType
        )
        {
            text = TextProcessor.CleanText(text);

            switch (textType)
            {
                case TextType.Dialogue:
                    if (!string.IsNullOrWhiteSpace(speaker))
                        return $"{speaker}: {text}";
                    return text;

                case TextType.MenuChoice:
                case TextType.Menu:
                case TextType.SystemMessage:
                case TextType.PoetryGame:
                    return text;

                case TextType.Poem:
                    if (!string.IsNullOrWhiteSpace(speaker))
                        return $"{speaker}: {text}";
                    return text;

                default:
                    return text;
            }
        }
    }

    public enum TextType
    {
        Dialogue,
        MenuChoice,
        Menu,
        SystemMessage,
        Narrator,
        PoetryGame,
        FileBrowser,
        Poem,
    }
}
