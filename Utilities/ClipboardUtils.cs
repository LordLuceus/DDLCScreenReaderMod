using System;

namespace DDLCScreenReaderMod
{
    public static class ClipboardUtils
    {
        private static string _currentDialogueSpeaker = "";
        private static string _currentDialogueText = "";
        private static TextType _currentDialogueType = TextType.Dialogue;

        private static string _lastSpokenMessage = "";
        private static DateTime _lastSpeakTime = DateTime.MinValue;
        private const double DuplicateWindowSeconds = 0.5;

        private static void SpeakText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            DateTime now = DateTime.UtcNow;
            if (
                text == _lastSpokenMessage
                && (now - _lastSpeakTime).TotalSeconds < DuplicateWindowSeconds
            )
            {
                return;
            }

            _lastSpokenMessage = text;
            _lastSpeakTime = now;

            UniversalSpeechWrapper.Speak(text, interrupt: false);
        }

        public static void RepeatCurrentDialogue()
        {
            if (!string.IsNullOrWhiteSpace(_currentDialogueText))
            {
                string formattedText = FormatTextForScreenReader(
                    _currentDialogueSpeaker,
                    _currentDialogueText,
                    _currentDialogueType
                );
                SpeakText(formattedText);
                ScreenReaderMod.Logger?.Msg($"Repeating dialogue: '{formattedText}'");
            }
            else
            {
                ScreenReaderMod.Logger?.Msg("No dialogue available to repeat");
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

            if (textType == TextType.Dialogue || textType == TextType.Narrator)
            {
                _currentDialogueSpeaker = speaker ?? "";
                _currentDialogueText = text;
                _currentDialogueType = textType;
            }

            SpeakText(formattedText);

            ScreenReaderMod.Logger?.Msg(
                $"[{textType}] Speaking: '{formattedText}' (Speaker: '{speaker}')"
            );
        }

        public static void OutputPoemText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            string formattedText = FormatTextForScreenReader("", text, TextType.Poem);

            _currentDialogueSpeaker = "";
            _currentDialogueText = text;
            _currentDialogueType = TextType.Poem;

            SpeakText(formattedText);
            ScreenReaderMod.Logger?.Msg($"[Poem] Speaking: '{formattedText}'");
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
        Settings,
        Mail,
        Jukebox,
    }
}
