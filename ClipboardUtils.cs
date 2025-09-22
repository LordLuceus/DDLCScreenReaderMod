using System.Collections.Generic;

namespace DDLCScreenReaderMod
{
    public static class ClipboardUtils
    {
        private static readonly Queue<string> MessageQueue = new Queue<string>();

        private static string _currentDialogueSpeaker = "";
        private static string _currentDialogueText = "";
        private static TextType _currentDialogueType = TextType.Dialogue;

        public static string DequeueMessage()
        {
            if (MessageQueue.Count > 0)
            {
                return MessageQueue.Dequeue();
            }
            return null;
        }

        private static void EnqueueText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            MessageQueue.Enqueue(text);
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
                EnqueueText(formattedText);
                ScreenReaderMod.Logger?.Msg($"Re-queued dialogue for repeat: '{formattedText}'");
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

            EnqueueText(formattedText);

            ScreenReaderMod.Logger?.Msg(
                $"[{textType}] Queued: '{formattedText}' (Speaker: '{speaker}')"
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

            EnqueueText(formattedText);
            ScreenReaderMod.Logger?.Msg($"[Poem] Queued: '{formattedText}'");
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
    }
}
