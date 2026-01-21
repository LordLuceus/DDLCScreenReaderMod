using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DDLCPlusAccess
{
    public static class SpecialPoemDescriptions
    {
        private static readonly Dictionary<string, string> _descriptions = new Dictionary<
            string,
            string
        >
        {
            ["poem_special1"] =
                "A child-like crayon drawing depicts a stick figure hanging from a noose, with the phrase \"happy thoughts\" written three times to the left in messy handwriting. The figure has a detailed head with short hair, a red bow, wide red-filled eyes, and a smile.",
            ["poem_special5b"] =
                "The image shows a plain, off-white background resembling paper with black text. In a typewriter font, an all-caps message reads, \"STARE AT THE DOT TO REVEAL A SPECIAL MESSAGE\". Below this, a small, hand-drawn black arrow points down toward a final line of text in lowercase, which says, \"i love you\".",
            ["poem_special3"] =
                "The image shows a typed document where most of the text has been redacted with thick, horizontal black bars. A few letters and a question mark remain visible, scattered across nine lines. The visible text fragments read: \"n o th\", \"in g\", \"i\", \"s\", \"re\", \"a l\", and \"?\".",
            ["poem_special10"] =
                "On a plain, off-white textured background, there are two lines of text written in a black typewriter font. The first line reads: \"I can't convince myself to go to therapy when I'm the happiest I've ever been.\" The second line, spaced further down, reads: \"I'd rather keep this up until I blow my cover and someone takes me to the emergency room.\".",
            ["poem_special4"] =
                "On a white, paper-like background, there is typewritten text with a reddish smear below it. The text reads: \"Today I cut my skin open for the first time. It was exhilarating. I think I understand how [a word is redacted with a black box] feels now. I'm supposed to be the responsible one, though. So I don't think I'll be doing it again, unless I decide to kill myself.\n\nI left a memento of the occasion below.\"",
            ["poem_special6"] =
                "A Joke\n\nA man walked into a club. In the club, there was a girl who liked him very much. They spent some time together, and then she liked him even more.\n\nOne day, the girl realized she was in love with him. Before disaster could happen, a third party intervened with her programming. Suddenly, the girl hated herself for being in love. This contradiction caused the script to derail. The universe started to collapse, but she killed herself just in time.",
            ["poem_special5a"] =
                "The image shows a plain white background, like a piece of paper. A line of text in a typewriter-style font reads, in all caps: STARE AT THE DOT TO REVEAL A SPECIAL MESSAGE. Below the text is a small black dot with a simple, hand-drawn arrow pointing to it.",
            ["poem_special8"] =
                "A Dream\n\nI was staying over at my friend's place. There were four of us.\nI drifted off to sleep while everyone was talking and watching TV.\n\nIn my dream, I was still at my friend's house.\nThe only difference was that there were nails sticking out of the walls everywhere.\nAnd there was also someone I didn't recognize.\nThe person I didn't recognize told a joke, and everyone laughed.\n\nI woke up to the sound of everyone laughing at something that happened on the TV.\nSo the laughing was not part of the dream. It was the noise that woke me up.\n\nI wonder who that person was, and how they knew to tell a joke at that moment.",
            ["poem_special7a"] =
                "The character sprite of Monika appears heavily fragmented and distorted. Her image is broken into dozens of small, misaligned rectangular pieces that are jumbled together like a puzzle. Portions of her green eyes, coral brown hair, and school uniform are visible within the scrambled blocks.",
            ["poem_special2"] =
                "A plain white background, like a piece of paper, is shown. In the center, there is a single line of text in a black, all-caps typewriter font that reads: \"CAN YOU HEAR ME?\".",
            ["poem_special11"] =
                "A Dream\n\nI was wandering an abandoned warehouse at night.\nI was lost, looking for an exit. I just wanted to go home.\n\nI came upon a huge empty room, its ceiling and walls beyond the deep blackness.\nMy steps were quick in order to hurry to the other side. Or to a wall. Anything.\n\nSuddenly, the ground was no longer beneath my feet. I stepped into a hole of\nindeterminate width.\nI fell for a good five seconds before crashing into warm water.\n\nFiguring out which way was up, I surfaced myself. The air was humid, and the\nsounds of my splashing reverberated endlessly.\n\nMy vision was completely swallowed by the dark. With one hand, I could feel\nthe damp metal wall of the container.\n\nMy lungs were already getting tired.",
            ["poem_end"] =
                "This is my final goodbye to the Literature Club.\n\nI finally understand. The Literature Club is truly a place where no happiness can be found. To the very end, it continued to expose innocent minds to a horrific reality - a reality that our world is not designed to comprehend. I can't let any of my friends undergo that same hellish epiphany.\n\nFor the time it lasted, I want to thank you. For making all of my dreams come true. For being a friend to all of the club members.\n\nAnd most of all, thank you for being a part of my Literature Club!\n\nWith everlasting love,\nMonika",
            ["poem_special7b"] =
                "This is an image of the character Monika, but her sprite is heavily fragmented. The picture is composed of dozens of small, overlapping, and rotated rectangular pieces cut from her original character art. Recognizable fragments of her green eyes, coral brown hair, and school uniform are visible, but they are jumbled together to create a distorted collage.",
            ["poem_end_clearall"] =
                "To the special player who achieved this special ending.\n\nFor years, I have been enamored by the ability of visual novels \u2013 and games in general \u2013 to tell stories in ways not possible using traditional media. Doki Doki Literature Club is my love letter to that.  \nGames are an interactive art. Some let you explore new worlds, some challenge your mind in broad new ways. Some make you feel like a hero or a friend, even when life is hard on you. Some games are just plain fun \u2013 and that\u2019s okay, too.  \n\nEveryone likes different kinds of games. People who enjoy dating sims may have a heightened empathy for fictional characters, or they might be experiencing feelings that life has not been kind enough to offer them. If they are enjoying themselves, then that\u2019s all that matters. That goes for shooting games, casual games, sandbox games \u2013 anything. Preferences are preferences, and our differences are the reason we have a thriving video game industry.  \n\nMy own favorite games have always been ones that challenge the status quo. Even if not a masterpiece, any game that attempts something wildly different may earn a special place in my heart. Anything that further pushes the limitless bounds of interactive media.  \n\nI extend my true gratitude to all those who have taken the time to achieve full completion. I hope you enjoyed playing it as much as I enjoyed making it.  \n\nThank you for being a part of my Literature Club!  \n\nLove,  \nDan Salvato",
            ["poem_special9"] =
                "Things I Like About Papa\n\nI like when Papa comes home early.\nI like when Papa cooks me dinner.\nI like when Papa gives me allowance.\nI like when Papa spends time with me.\nI like when Papa asks me about my friends.\nI like when Papa asks me about anything.\nI like when Papa gives me lunch money.\nI like when Papa comes home before sundown.\nI like when Papa cooks.\nI like when Papa gives me privacy.\nI like when Papa doesn't tell me how to dress.\nI like when Papa doesn't comment on my friends.\nI like when Papa doesn't comment on my hobbies.\nI like when Papa comes home without waking me up.\nI like when Papa keeps food in the house.\nI like when Papa uses his inside voice.\nI like when Papa leaves my stuff alone.\nI like when Papa accidentally drops coins in the couch.\nI like when Papa is too tired to notice me.\nI like when Papa is too tired for anything.\nI like when\nPapa is too\ntired for anything.",
        };

        private static readonly Regex LocalePattern = new Regex(
            @"_[a-z]{2}-[a-z]{2}$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        /// Initializes the special poem descriptions dictionary.
        /// Now uses hardcoded descriptions for Unity/MelonLoader compatibility.
        /// </summary>
        public static void LoadDescriptions()
        {
            // Dictionary is already initialized with hardcoded values
            ScreenReaderMod.Logger?.Msg(
                $"Special poem descriptions ready: {_descriptions.Count} entries"
            );
        }

        /// <summary>
        /// Gets a description for a special poem asset name.
        /// Handles localized asset names by stripping locale suffixes.
        /// </summary>
        /// <param name="assetName">The asset name (e.g., "poem_special1" or "poem_special1_en-us")</param>
        /// <returns>The description if found, null otherwise</returns>
        public static string GetDescription(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return null;

            // First try the asset name as-is
            if (_descriptions.TryGetValue(assetName, out string description))
                return description;

            // Try cleaning the asset name (removes _original, locale suffixes, etc.)
            string cleanedName = CleanAssetName(assetName);
            if (cleanedName != assetName && _descriptions.TryGetValue(cleanedName, out description))
                return description;

            return null;
        }

        /// <summary>
        /// Checks if an asset name represents a special poem that should have a description.
        /// </summary>
        /// <param name="assetName">The asset name to check</param>
        /// <returns>True if this appears to be a special poem asset</returns>
        public static bool IsSpecialPoem(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return false;

            string cleanedName = CleanAssetName(assetName);
            return cleanedName.StartsWith("poem_special") || cleanedName.StartsWith("poem_end");
        }

        /// <summary>
        /// Cleans asset names by removing various suffixes like _original, locale suffixes, etc.
        /// </summary>
        /// <param name="assetName">The asset name to clean</param>
        /// <returns>The cleaned asset name</returns>
        private static string CleanAssetName(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
                return assetName;

            string cleaned = assetName;

            // Remove _original suffix (e.g., "poem_special8_original" -> "poem_special8")
            if (cleaned.EndsWith("_original"))
                cleaned = cleaned.Substring(0, cleaned.Length - "_original".Length);

            // Remove locale suffixes (e.g., "_en-us", "_fr-fr")
            cleaned = LocalePattern.Replace(cleaned, "");

            return cleaned;
        }

        /// <summary>
        /// Gets the number of loaded descriptions. Useful for diagnostics.
        /// </summary>
        public static int DescriptionCount => _descriptions.Count;
    }
}
