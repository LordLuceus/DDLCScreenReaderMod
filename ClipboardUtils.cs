using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MelonLoader;

namespace DDLCScreenReaderMod
{
    public static class ClipboardUtils
    {
        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

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

                if (SetClipboardText(formattedText))
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

        public static bool SetClipboardText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text == lastText && DateTime.Now - lastUpdate < MinUpdateInterval)
                return false;

            try
            {
                return SetClipboardTextInternal(text);
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Failed to set clipboard text: {ex.Message}");
                return false;
            }
        }

        private static bool SetClipboardTextInternal(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
                return false;

            try
            {
                EmptyClipboard();

                byte[] bytes = Encoding.Unicode.GetBytes(text + "\0");
                IntPtr hGlobal = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes.Length);

                if (hGlobal == IntPtr.Zero)
                    return false;

                try
                {
                    IntPtr lpMem = GlobalLock(hGlobal);
                    if (lpMem != IntPtr.Zero)
                    {
                        Marshal.Copy(bytes, 0, lpMem, bytes.Length);
                        GlobalUnlock(hGlobal);

                        if (SetClipboardData(CF_UNICODETEXT, hGlobal) != IntPtr.Zero)
                        {
                            lastText = text;
                            lastUpdate = DateTime.Now;
                            return true;
                        }
                    }

                    GlobalFree(hGlobal);
                    return false;
                }
                catch
                {
                    GlobalFree(hGlobal);
                    throw;
                }
            }
            finally
            {
                CloseClipboard();
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

            if (!ModConfig.Instance.ShouldOutputTextType(textType))
                return;

            if (text.Length > ModConfig.Instance.MaxTextLength)
            {
                text = text.Substring(0, ModConfig.Instance.MaxTextLength) + "...";
            }

            string formattedText = FormatTextForScreenReader(speaker, text, textType);

            if (SetClipboardText(formattedText))
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

                if (ModConfig.Instance.EnableVerboseLogging)
                {
                    ScreenReaderMod.Logger?.Msg(
                        $"[{textType}] Output to clipboard: {formattedText}"
                    );
                }
            }
        }

        private static string FormatTextForScreenReader(
            string speaker,
            string text,
            TextType textType
        )
        {
            // Clean the text (re-enable text processing now that we have filtering)
            text = TextProcessor.CleanText(text);

            switch (textType)
            {
                case TextType.Dialogue:
                    if (
                        ModConfig.Instance.IncludeSpeakerNames
                        && !string.IsNullOrWhiteSpace(speaker)
                    )
                        return $"{speaker}: {text}";
                    return text;

                case TextType.MenuChoice:
                    return text;

                case TextType.Menu:
                    return text;

                case TextType.SystemMessage:
                    return $"System: {text}";

                case TextType.Narrator:
                    return text;

                case TextType.PoetryGame:
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
    }
}
