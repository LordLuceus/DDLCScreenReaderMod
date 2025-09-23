using System.Collections.Generic;

namespace DDLCScreenReaderMod
{
    public static class GalleryImageDescriptions
    {
        private static readonly Dictionary<string, string> descriptions = new Dictionary<
            string,
            string
        >
        {
            ["gallery_poem_dearsunshine"] =
                "Dear Sunshine\n\nThe way you glow through my blinds in the morning\nIt makes me feel like you missed me.\nKissing my forehead to help me out of bed.\nMaking me rub the sleepy from my eyes.\n\nAre you asking me to come out and play?\nAre you trusting me to wish away a rainy day?\nI look above. The sky is blue.\nIt's a secret, but I trust you too.\n\nIf it wasn't for you, I could sleep forever.\nBut I'm not mad.\n\nI want breakfast.",
        };

        public static string GetDescription(string imageName)
        {
            if (descriptions.TryGetValue(imageName, out string description))
            {
                return description;
            }
            return "";
        }
    }
}
