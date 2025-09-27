using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class GalleryPatches
    {
        // Track recent announcements to prevent duplicates
        private static string lastAnnouncedThumbnail = "";
        private static System.DateTime lastThumbnailTime = System.DateTime.MinValue;
        private static readonly System.TimeSpan ThumbnailDuplicateWindow =
            System.TimeSpan.FromMilliseconds(1000);

        // Track image navigation to prevent announcing when no actual navigation occurs
        private static int lastImageIndex = -1;

        private static bool IsThumbnailDuplicate(string thumbnailKey)
        {
            var now = System.DateTime.Now;
            if (
                thumbnailKey == lastAnnouncedThumbnail
                && (now - lastThumbnailTime) < ThumbnailDuplicateWindow
            )
            {
                return true;
            }

            lastAnnouncedThumbnail = thumbnailKey;
            lastThumbnailTime = now;
            return false;
        }

        /// <summary>
        /// Build a comprehensive image description message including description, unlock condition, and wallpaper status
        /// </summary>
        private static string BuildImageMessage(
            GalleryConfiguration.GalleryEntry entry,
            GalleryApp galleryInstance,
            string prefix = ""
        )
        {
            if (entry == null)
                return "Unknown image";

            List<string> parts = new List<string>();

            string entryName = entry.ImageRef?.AssetName ?? "Unknown image";
            parts.Add(entryName);

            // Add description if available
            if (!string.IsNullOrEmpty(entry.Description))
            {
                parts.Add(entry.Description);
            }

            // Add unlock condition if available
            if (!string.IsNullOrEmpty(entry.UnlockCondition))
            {
                parts.Add($"Unlock condition: {entry.UnlockCondition}");
            }

            // Add wallpaper status by checking directly with the launcher
            if (entry.ImageRef != null)
            {
                bool isCurrentWallpaper = Renpy.LauncherMain.IsCurrentWallpaper(
                    entry.ImageRef.AssetName,
                    entry.ImageRef.AssetBundleName
                );
                if (isCurrentWallpaper)
                {
                    parts.Add("Currently set as wallpaper");
                }
            }

            if (DescriptionManager.HasDescription(entryName))
            {
                parts.Add("Press enter to read full image description");
            }

            string message = string.Join(
                " ",
                parts.ConvertAll(part =>
                    part.EndsWith(".") || part.EndsWith("!") || part.EndsWith("?")
                        ? part
                        : part + "."
                )
            );

            return string.IsNullOrEmpty(prefix) ? message : $"{prefix} {message}";
        }

        /// <summary>
        /// Announce when the gallery app starts
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "OnAppStart")]
        [HarmonyPostfix]
        public static void GalleryApp_OnAppStart_Postfix(GalleryApp __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Gallery app opened");
                ClipboardUtils.OutputGameText("", "Gallery opened", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_OnAppStart_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when view changes between Thumbnails/Preview/Fullscreen
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "SetCurrentView")]
        [HarmonyPostfix]
        public static void GalleryApp_SetCurrentView_Postfix(GalleryApp __instance, object view)
        {
            try
            {
                string viewName = view.ToString();

                // Handle fullscreen view with image description
                if (viewName == "FullScreen")
                {
                    // Get current image info for description
                    var currentIndexField = AccessTools.Field(
                        typeof(GalleryApp),
                        "m_CurrentImageIndex"
                    );
                    var unlockedImagesField = AccessTools.Field(
                        typeof(GalleryApp),
                        "m_UnlockedImages"
                    );

                    if (currentIndexField != null && unlockedImagesField != null)
                    {
                        int currentIndex = (int)currentIndexField.GetValue(__instance);
                        var unlockedImages =
                            unlockedImagesField.GetValue(__instance)
                            as List<GalleryConfiguration.GalleryEntry>;

                        if (
                            unlockedImages != null
                            && currentIndex >= 0
                            && currentIndex < unlockedImages.Count
                        )
                        {
                            var currentEntry = unlockedImages[currentIndex];
                            string imageName = currentEntry.ImageRef?.AssetName ?? "Unknown image";

                            // Get full description if available
                            string fullDescription = DescriptionManager.GetDescription(imageName);

                            string fullscreenMessage;
                            if (!string.IsNullOrEmpty(fullDescription))
                            {
                                fullscreenMessage =
                                    $"Fullscreen view: {imageName}. {fullDescription}";
                            }
                            else
                            {
                                fullscreenMessage = "Fullscreen image view";
                            }

                            ScreenReaderMod.Logger?.Msg(
                                $"Gallery view changed to: {viewName} - {imageName}"
                            );
                            ClipboardUtils.OutputGameText(
                                "",
                                fullscreenMessage,
                                TextType.SystemMessage
                            );
                            return;
                        }
                    }
                }

                // Handle other view changes
                string message = viewName switch
                {
                    "Thumbnails" => "Gallery thumbnails view",
                    "Preview" => "Image preview view",
                    "FullScreen" => "Fullscreen image view", // Fallback if we couldn't get image info
                    _ => $"{viewName} view",
                };

                ScreenReaderMod.Logger?.Msg($"Gallery view changed to: {viewName}");
                ClipboardUtils.OutputGameText("", message, TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_SetCurrentView_Postfix: {ex.Message}"
                );
            }
        }

        // Track current section to announce section changes only when crossing section boundaries
        private static string currentSectionName = "";
        private static string lastSelectedThumbnailId = "";

        /// <summary>
        /// Announce section changes when navigating between thumbnails
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "HandleSelection")]
        [HarmonyPostfix]
        public static void GalleryApp_HandleSelection_Postfix(GalleryApp __instance)
        {
            try
            {
                // Get the currently selected thumbnail using EventSystem
                var currentSelected = EventSystem.current?.currentSelectedGameObject;
                if (currentSelected == null)
                    return;

                var sceneThumbnail = currentSelected.GetComponent<SceneThumbnail>();
                if (sceneThumbnail?.gallerySection?.SectionTitle?.text == null)
                    return;

                string sectionName = sceneThumbnail.gallerySection.SectionTitle.text;
                string thumbnailId = sceneThumbnail.EntryInfo?.ImageRef?.AssetName ?? "";

                // Only announce section change when entering a new section
                if (sectionName != currentSectionName)
                {
                    currentSectionName = sectionName;

                    // Count total items in this section
                    int itemCount = 0;
                    if (sceneThumbnail.gallerySection.ThumbnailGrid?.transform != null)
                    {
                        itemCount = sceneThumbnail
                            .gallerySection
                            .ThumbnailGrid
                            .transform
                            .childCount;
                    }

                    string message =
                        $"{sectionName} section, {itemCount} item{(itemCount != 1 ? "s" : "")}";

                    ScreenReaderMod.Logger?.Msg($"Gallery section entered: {message}");
                    ClipboardUtils.OutputGameText("", message, TextType.Menu);
                }

                // Also announce the current thumbnail if it changed (but not if we just announced the section)
                if (thumbnailId != lastSelectedThumbnailId && sectionName == currentSectionName)
                {
                    lastSelectedThumbnailId = thumbnailId;

                    if (!string.IsNullOrEmpty(thumbnailId))
                    {
                        string positionInfo = "";
                        if (sceneThumbnail.elementIndex >= 0)
                        {
                            positionInfo = $", position {sceneThumbnail.elementIndex + 1}";
                        }

                        string message = $"{thumbnailId}{positionInfo}";
                        ClipboardUtils.OutputGameText("", message, TextType.Menu);
                    }
                }

                lastSelectedThumbnailId = thumbnailId;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_HandleSelection_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when a thumbnail is selected
        /// </summary>
        [HarmonyPatch(typeof(SceneThumbnail), "OnSelected")]
        [HarmonyPostfix]
        public static void SceneThumbnail_OnSelected_Postfix(SceneThumbnail __instance)
        {
            try
            {
                if (__instance?.EntryInfo == null)
                    return;

                var entry = __instance.EntryInfo;
                string imageName = entry.ImageRef?.AssetName ?? "Unknown image";

                // Check for duplicates
                if (IsThumbnailDuplicate(imageName))
                {
                    return;
                }

                // Get section and position info
                string sectionInfo = "";
                if (__instance.gallerySection?.SectionTitle?.text != null)
                {
                    sectionInfo = $" from {__instance.gallerySection.SectionTitle.text}";
                }

                string positionInfo = "";
                if (__instance.elementIndex >= 0)
                {
                    positionInfo = $", position {__instance.elementIndex + 1}";
                }

                string message = $"Selected {imageName}{sectionInfo}{positionInfo}";

                ScreenReaderMod.Logger?.Msg($"Gallery thumbnail selected: {imageName}");
                ClipboardUtils.OutputGameText("", message, TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in SceneThumbnail_OnSelected_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when entering preview mode with image details
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "ShowSelectedThumbnail")]
        [HarmonyPostfix]
        public static void GalleryApp_ShowSelectedThumbnail_Postfix(
            GalleryApp __instance,
            GalleryConfiguration.GalleryEntry entry
        )
        {
            try
            {
                if (entry == null)
                    return;

                string message = BuildImageMessage(entry, __instance, "Viewing");
                string entryName = entry.ImageRef?.AssetName ?? "Unknown image";

                // Initialize image index tracking when entering preview mode
                var currentIndexField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_CurrentImageIndex"
                );
                if (currentIndexField != null)
                {
                    lastImageIndex = (int)currentIndexField.GetValue(__instance);
                }

                ScreenReaderMod.Logger?.Msg($"Gallery preview opened for: {entryName}");
                ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_ShowSelectedThumbnail_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when navigating to next image with full details
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "NextImage")]
        [HarmonyPostfix]
        public static void GalleryApp_NextImage_Postfix(GalleryApp __instance)
        {
            try
            {
                // Access private fields using reflection
                var currentIndexField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_CurrentImageIndex"
                );
                var unlockedImagesField = AccessTools.Field(typeof(GalleryApp), "m_UnlockedImages");
                var isCurrentWallpaperField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_IsCurrentWallpaper"
                );

                if (currentIndexField != null && unlockedImagesField != null)
                {
                    int currentIndex = (int)currentIndexField.GetValue(__instance);
                    var unlockedImages =
                        unlockedImagesField.GetValue(__instance)
                        as List<GalleryConfiguration.GalleryEntry>;

                    // Only announce if the index actually changed
                    if (
                        unlockedImages != null
                        && currentIndex < unlockedImages.Count
                        && currentIndex != lastImageIndex
                    )
                    {
                        var currentEntry = unlockedImages[currentIndex];
                        string message = BuildImageMessage(currentEntry, __instance, "Next image:");

                        ScreenReaderMod.Logger?.Msg(
                            $"Gallery navigation: Next image {currentIndex + 1} of {unlockedImages.Count}"
                        );
                        ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);

                        // Update the tracked index
                        lastImageIndex = currentIndex;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_NextImage_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when navigating to previous image with full details
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "PreviousImage")]
        [HarmonyPostfix]
        public static void GalleryApp_PreviousImage_Postfix(GalleryApp __instance)
        {
            try
            {
                // Access private fields using reflection
                var currentIndexField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_CurrentImageIndex"
                );
                var unlockedImagesField = AccessTools.Field(typeof(GalleryApp), "m_UnlockedImages");
                var isCurrentWallpaperField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_IsCurrentWallpaper"
                );

                if (currentIndexField != null && unlockedImagesField != null)
                {
                    int currentIndex = (int)currentIndexField.GetValue(__instance);
                    var unlockedImages =
                        unlockedImagesField.GetValue(__instance)
                        as List<GalleryConfiguration.GalleryEntry>;

                    // Only announce if the index actually changed
                    if (
                        unlockedImages != null
                        && currentIndex >= 0
                        && currentIndex < unlockedImages.Count
                        && currentIndex != lastImageIndex
                    )
                    {
                        var currentEntry = unlockedImages[currentIndex];
                        string message = BuildImageMessage(
                            currentEntry,
                            __instance,
                            "Previous image:"
                        );

                        ScreenReaderMod.Logger?.Msg(
                            $"Gallery navigation: Previous image {currentIndex + 1} of {unlockedImages.Count}"
                        );
                        ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);

                        // Update the tracked index
                        lastImageIndex = currentIndex;
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_PreviousImage_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce when returning to thumbnails view
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "GoBackToThumbnails")]
        [HarmonyPostfix]
        public static void GalleryApp_GoBackToThumbnails_Postfix(GalleryApp __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Gallery: Returning to thumbnails view");
                ClipboardUtils.OutputGameText("", "Returned to thumbnails", TextType.Menu);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_GoBackToThumbnails_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce fullscreen mode toggle and image description
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "ToggleFullScreen")]
        [HarmonyPostfix]
        public static void GalleryApp_ToggleFullScreen_Postfix(GalleryApp __instance)
        {
            try
            {
                // Check if fullscreen is active by examining the FullScreen GameObject
                var fullScreenField = AccessTools.Field(typeof(GalleryApp), "FullScreen");
                if (fullScreenField != null)
                {
                    var fullScreenObject = fullScreenField.GetValue(__instance) as GameObject;
                    if (fullScreenObject != null)
                    {
                        bool isFullScreen = fullScreenObject.activeSelf;

                        if (isFullScreen)
                        {
                            ClipboardUtils.OutputGameText(
                                "",
                                "Entering fullscreen mode",
                                TextType.Menu
                            );
                        }
                        else
                        {
                            ClipboardUtils.OutputGameText(
                                "",
                                "Exiting fullscreen mode",
                                TextType.Menu
                            );
                        }

                        ScreenReaderMod.Logger?.Msg(
                            $"Gallery fullscreen toggle: {(isFullScreen ? "Entering" : "Exiting")}"
                        );
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_ToggleFullScreen_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce wallpaper setting action and current wallpaper status
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "SetWallpaper")]
        [HarmonyPrefix]
        public static void GalleryApp_SetWallpaper_Prefix(GalleryApp __instance)
        {
            try
            {
                // Get current image info
                var currentIndexField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_CurrentImageIndex"
                );
                var unlockedImagesField = AccessTools.Field(typeof(GalleryApp), "m_UnlockedImages");
                var isCurrentWallpaperField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_IsCurrentWallpaper"
                );

                if (
                    currentIndexField != null
                    && unlockedImagesField != null
                    && isCurrentWallpaperField != null
                )
                {
                    int currentIndex = (int)currentIndexField.GetValue(__instance);
                    var unlockedImages =
                        unlockedImagesField.GetValue(__instance)
                        as List<GalleryConfiguration.GalleryEntry>;
                    bool isCurrentWallpaper = (bool)isCurrentWallpaperField.GetValue(__instance);

                    if (
                        unlockedImages != null
                        && currentIndex >= 0
                        && currentIndex < unlockedImages.Count
                    )
                    {
                        var currentEntry = unlockedImages[currentIndex];
                        string imageName = currentEntry.ImageRef?.AssetName ?? "Unknown image";

                        string action = isCurrentWallpaper
                            ? "Clear current wallpaper"
                            : $"Set {imageName} as wallpaper";
                        string message = $"{action}. Confirmation dialog opened.";

                        ScreenReaderMod.Logger?.Msg($"Gallery wallpaper action: {action}");
                        ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_SetWallpaper_Prefix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce wallpaper confirmation result
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "ConfirmWallpaper")]
        [HarmonyPostfix]
        public static void GalleryApp_ConfirmWallpaper_Postfix(GalleryApp __instance)
        {
            try
            {
                // Get current image info
                var currentIndexField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_CurrentImageIndex"
                );
                var unlockedImagesField = AccessTools.Field(typeof(GalleryApp), "m_UnlockedImages");

                if (currentIndexField != null && unlockedImagesField != null)
                {
                    int currentIndex = (int)currentIndexField.GetValue(__instance);
                    var unlockedImages =
                        unlockedImagesField.GetValue(__instance)
                        as List<GalleryConfiguration.GalleryEntry>;

                    if (
                        unlockedImages != null
                        && currentIndex >= 0
                        && currentIndex < unlockedImages.Count
                    )
                    {
                        var currentEntry = unlockedImages[currentIndex];
                        string imageName = currentEntry.ImageRef?.AssetName ?? "Unknown image";

                        string message = $"Wallpaper set to {imageName}";

                        ScreenReaderMod.Logger?.Msg($"Gallery wallpaper confirmed: {imageName}");
                        ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_ConfirmWallpaper_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce wallpaper cancellation
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "CancelWallpaper")]
        [HarmonyPostfix]
        public static void GalleryApp_CancelWallpaper_Postfix(GalleryApp __instance)
        {
            try
            {
                ScreenReaderMod.Logger?.Msg("Gallery wallpaper setting cancelled");
                ClipboardUtils.OutputGameText(
                    "",
                    "Wallpaper setting cancelled",
                    TextType.SystemMessage
                );
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_CancelWallpaper_Postfix: {ex.Message}"
                );
            }
        }

        /// <summary>
        /// Announce wallpaper button text updates when navigating between images
        /// </summary>
        [HarmonyPatch(typeof(GalleryApp), "RefreshWallpaperButtonText", new System.Type[] { })]
        [HarmonyPostfix]
        public static void GalleryApp_RefreshWallpaperButtonText_Postfix(GalleryApp __instance)
        {
            try
            {
                // Get wallpaper button text to announce current status
                var wallpaperButtonTextField = AccessTools.Field(
                    typeof(GalleryApp),
                    "WallpaperButtonText"
                );
                var isCurrentWallpaperField = AccessTools.Field(
                    typeof(GalleryApp),
                    "m_IsCurrentWallpaper"
                );

                if (wallpaperButtonTextField != null && isCurrentWallpaperField != null)
                {
                    var wallpaperButtonText = wallpaperButtonTextField.GetValue(__instance);
                    bool isCurrentWallpaper = (bool)isCurrentWallpaperField.GetValue(__instance);

                    // Only announce wallpaper status when it changes, not every time the button text refreshes
                    string wallpaperStatus = isCurrentWallpaper
                        ? "Current wallpaper"
                        : "Available to set as wallpaper";

                    // We'll track this with the image navigation instead of announcing it every refresh
                    // This prevents spam while still providing the information when needed
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in GalleryApp_RefreshWallpaperButtonText_Postfix: {ex.Message}"
                );
            }
        }
    }
}
