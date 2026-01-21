using System.Collections.Generic;

namespace DDLCPlusAccess
{
    public static class DescriptionManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> sectionDescriptions =
            new Dictionary<string, Dictionary<string, string>>();

        static DescriptionManager()
        {
            // Register all section description dictionaries
            RegisterSection("Poems", PoemDescriptions.Descriptions);
            RegisterSection("Wallpapers", WallpaperDescriptions.Descriptions);
            RegisterSection("Secrets", SecretDescriptions.Descriptions);
            RegisterSection("Backgrounds", BackgroundDescriptions.Descriptions);
            RegisterSection("Promos", PromoDescriptions.Descriptions);
            RegisterSection("CGs", CGDescriptions.Descriptions);
            RegisterSection("Sketches", SketchDescriptions.Descriptions);
        }

        private static void RegisterSection(
            string sectionName,
            Dictionary<string, string> descriptions
        )
        {
            sectionDescriptions[sectionName] = descriptions;
        }

        /// <summary>
        /// Get description for an image by checking all sections
        /// </summary>
        /// <param name="imageName">The asset name of the image</param>
        /// <returns>Description if found, empty string otherwise</returns>
        public static string GetDescription(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return "";

            // Check all sections for the image
            foreach (var sectionDict in sectionDescriptions.Values)
            {
                if (sectionDict.TryGetValue(imageName, out string description))
                {
                    return description;
                }
            }

            return "";
        }

        /// <summary>
        /// Get description for an image from a specific section
        /// </summary>
        /// <param name="sectionName">The gallery section name</param>
        /// <param name="imageName">The asset name of the image</param>
        /// <returns>Description if found, empty string otherwise</returns>
        public static string GetDescription(string sectionName, string imageName)
        {
            if (string.IsNullOrEmpty(imageName) || string.IsNullOrEmpty(sectionName))
                return "";

            if (sectionDescriptions.TryGetValue(sectionName, out var sectionDict))
            {
                if (sectionDict.TryGetValue(imageName, out string description))
                {
                    return description;
                }
            }

            return "";
        }

        /// <summary>
        /// Check if an image has a description available
        /// </summary>
        /// <param name="imageName">The asset name of the image</param>
        /// <returns>True if description exists, false otherwise</returns>
        public static bool HasDescription(string imageName)
        {
            return !string.IsNullOrEmpty(GetDescription(imageName));
        }

        /// <summary>
        /// Get all available sections
        /// </summary>
        /// <returns>List of section names</returns>
        public static List<string> GetSections()
        {
            return new List<string>(sectionDescriptions.Keys);
        }

        /// <summary>
        /// Get count of descriptions in a specific section
        /// </summary>
        /// <param name="sectionName">The gallery section name</param>
        /// <returns>Number of descriptions in the section</returns>
        public static int GetSectionCount(string sectionName)
        {
            if (sectionDescriptions.TryGetValue(sectionName, out var sectionDict))
            {
                return sectionDict.Count;
            }
            return 0;
        }
    }
}
