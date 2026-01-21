using HarmonyLib;
using RenpyParser;
using RenPyParser.Transforms;
using UnityEngine;

namespace DDLCPlusAccess
{
    [HarmonyPatch]
    public static class ImagePatches
    {
        // Track recent images to prevent duplicates
        private static string lastImageKey = "";
        private static System.DateTime lastImageTime = System.DateTime.MinValue;
        private static readonly System.TimeSpan ImageDuplicateWindow =
            System.TimeSpan.FromMilliseconds(500);

        private static bool IsImageDuplicate(string imageKey)
        {
            var now = System.DateTime.Now;
            if (imageKey == lastImageKey && (now - lastImageTime) < ImageDuplicateWindow)
            {
                return true;
            }

            lastImageKey = imageKey;
            lastImageTime = now;
            return false;
        }

        [HarmonyPatch(
            typeof(RenpyLoadImage),
            "Immediate",
            typeof(GameObject),
            typeof(CurrentTransform),
            typeof(string)
        )]
        [HarmonyPostfix]
        public static void RenpyLoadImage_Immediate_Postfix(
            GameObject gameObject,
            CurrentTransform currentTransform,
            string key
        )
        {
            try
            {
                // Check if this is a special poem image
                if (string.IsNullOrWhiteSpace(key) || !SpecialPoemDescriptions.IsSpecialPoem(key))
                    return;

                // Prevent duplicate announcements
                if (IsImageDuplicate(key))
                    return;

                // Get description for this special poem
                string description = SpecialPoemDescriptions.GetDescription(key);
                if (!string.IsNullOrWhiteSpace(description))
                {
                    // Output the description as a poem
                    ClipboardUtils.OutputPoemText(description);
                    ScreenReaderMod.Logger?.Msg(
                        $"Output special poem description for image: {key}"
                    );
                }
                else
                {
                    ScreenReaderMod.Logger?.Warning(
                        $"No description found for special poem image: {key}"
                    );
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in RenpyLoadImage_Immediate_Postfix: {ex.Message}"
                );
            }
        }
    }
}
