using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RenpyLauncher;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class FileBrowserPatches
    {
        // File browser state tracking
        private static string lastSelectedFileItem = "";
        private static string lastAnnouncedDirectory = "";

        // File Browser Accessibility Patches

        // Patch file/folder selection to announce selected items
        [HarmonyPatch(typeof(FileBrowserApp), "ButtonStateChanged")]
        [HarmonyPostfix]
        public static void FileBrowserApp_ButtonStateChanged_Postfix(
            FileBrowserApp __instance,
            FileBrowserButton newSelection,
            FileBrowserButton.State selected
        )
        {
            try
            {
                if (selected == FileBrowserButton.State.Selected && newSelection != null)
                {
                    string itemText = FormatFileBrowserItem(newSelection);
                    if (!string.IsNullOrWhiteSpace(itemText) && itemText != lastSelectedFileItem)
                    {
                        lastSelectedFileItem = itemText;

                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg(
                                $"File browser item selected: {itemText}"
                            );

                        ClipboardUtils.OutputGameText("", itemText, TextType.FileBrowser);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileBrowserApp_ButtonStateChanged_Postfix: {ex.Message}"
                );
            }
        }

        // Patch update loop to announce directory changes after buttons are populated
        [HarmonyPatch(typeof(FileBrowserApp), "OnAppUpdate")]
        [HarmonyPostfix]
        public static void FileBrowserApp_OnAppUpdate_Postfix(FileBrowserApp __instance)
        {
            try
            {
                // Only announce if we have a directory change and buttons are ready
                if (__instance != null && IsInputEnabled(__instance))
                {
                    string currentFolder = GetCurrentFolder(__instance);
                    if (currentFolder != null)
                    {
                        string directoryInfo = GetDirectoryAnnouncement(__instance, currentFolder);
                        if (
                            !string.IsNullOrWhiteSpace(directoryInfo)
                            && directoryInfo != lastAnnouncedDirectory
                        )
                        {
                            lastAnnouncedDirectory = directoryInfo;

                            if (ModConfig.Instance.EnableVerboseLogging)
                                ScreenReaderMod.Logger?.Msg(
                                    $"File browser directory changed: {directoryInfo}"
                                );

                            ClipboardUtils.OutputGameText("", directoryInfo, TextType.FileBrowser);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileBrowserApp_OnAppUpdate_Postfix: {ex.Message}"
                );
            }
        }

        // Patch context menu operations to announce file actions
        [HarmonyPatch(typeof(FileBrowserApp), "OnContextMenuOpenClicked")]
        [HarmonyPrefix]
        public static void FileBrowserApp_OnContextMenuOpenClicked_Prefix(FileBrowserApp __instance)
        {
            try
            {
                var currentSelectedItem = GetCurrentSelectedItem(__instance);
                if (currentSelectedItem != null)
                {
                    string fileName = GetFileNameFromButton(currentSelectedItem);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string message = $"Opening {fileName}";

                        // Add information about Notepad integration if enabled
                        if (ModConfig.Instance.EnableNotepadIntegration)
                        {
                            message += ". File content will be opened in Notepad.";
                        }

                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg($"File browser opening: {message}");

                        ClipboardUtils.OutputGameText("", message, TextType.FileBrowser);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileBrowserApp_OnContextMenuOpenClicked_Prefix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(FileBrowserApp), "OnContextMenuDeleteClicked")]
        [HarmonyPrefix]
        public static void FileBrowserApp_OnContextMenuDeleteClicked_Prefix(
            FileBrowserApp __instance
        )
        {
            try
            {
                var currentSelectedItem = GetCurrentSelectedItem(__instance);
                if (currentSelectedItem != null)
                {
                    string fileName = GetFileNameFromButton(currentSelectedItem);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string message = $"Deleting {fileName}";

                        if (ModConfig.Instance.EnableVerboseLogging)
                            ScreenReaderMod.Logger?.Msg($"File browser deleting: {message}");

                        ClipboardUtils.OutputGameText("", message, TextType.FileBrowser);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileBrowserApp_OnContextMenuDeleteClicked_Prefix: {ex.Message}"
                );
            }
        }

        // Helper methods for file browser accessibility

        private static string FormatFileBrowserItem(FileBrowserButton button)
        {
            if (button == null)
                return null;

            try
            {
                string fileName = button.FileName ?? "";
                string fileSize = button.TextFileSizeComponent?.text ?? "";
                string fileType = button.TextFileTypeComponent?.text ?? "";
                string fileDate = button.TextFileDateComponent?.text ?? "";

                // Check if it's a folder
                if (button.UserData?.StartsWith("###") == true)
                {
                    return $"Folder: {fileName}";
                }

                // Format file information
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(fileType))
                    parts.Add(fileType);

                parts.Add(fileName);

                if (!string.IsNullOrWhiteSpace(fileSize) && fileSize != "--")
                    parts.Add($"Size: {fileSize}");

                return string.Join(", ", parts);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error formatting file browser item: {ex.Message}");
                return button.FileName ?? "Unknown file";
            }
        }

        private static string GetDirectoryAnnouncement(FileBrowserApp fileBrowserApp, string folder)
        {
            try
            {
                // Get directory path display
                string directoryPath = string.IsNullOrEmpty(folder)
                    ? "Root"
                    : folder.Replace('/', '\\');

                // Count items if possible
                string itemCount = GetDirectoryItemCount(fileBrowserApp);

                if (!string.IsNullOrEmpty(itemCount))
                    return $"Directory: {directoryPath}, {itemCount}";
                else
                    return $"Directory: {directoryPath}";
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error getting directory announcement: {ex.Message}"
                );
                return string.IsNullOrEmpty(folder) ? "Root directory" : $"Directory: {folder}";
            }
        }

        private static string GetDirectoryItemCount(FileBrowserApp fileBrowserApp)
        {
            try
            {
                var buttons = GetButtons(fileBrowserApp);
                if (buttons == null)
                    return null;

                int totalCount = 0;
                int folderCount = 0;
                int fileCount = 0;

                foreach (var button in buttons)
                {
                    if (button.gameObject.activeSelf)
                    {
                        totalCount++;
                        if (button.UserData?.StartsWith("###") == true)
                            folderCount++;
                        else
                            fileCount++;
                    }
                }

                if (totalCount == 0)
                    return "Empty";

                var parts = new List<string>();
                if (fileCount > 0)
                    parts.Add($"{fileCount} file{(fileCount == 1 ? "" : "s")}");
                if (folderCount > 0)
                    parts.Add($"{folderCount} folder{(folderCount == 1 ? "" : "s")}");

                return string.Join(", ", parts);
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error counting directory items: {ex.Message}");
                return null;
            }
        }

        // Helper methods using reflection to access private fields
        private static FileBrowserButton GetCurrentSelectedItem(FileBrowserApp instance)
        {
            try
            {
                var field = typeof(FileBrowserApp).GetField(
                    "m_CurrentSelectedItem",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                return field?.GetValue(instance) as FileBrowserButton;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting current selected item: {ex.Message}");
                return null;
            }
        }

        private static List<FileBrowserButton> GetButtons(FileBrowserApp instance)
        {
            try
            {
                var field = typeof(FileBrowserApp).GetField(
                    "m_Buttons",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                return field?.GetValue(instance) as List<FileBrowserButton>;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting buttons list: {ex.Message}");
                return null;
            }
        }

        private static bool IsInputEnabled(FileBrowserApp instance)
        {
            try
            {
                var field = typeof(FileBrowserApp).GetField(
                    "m_inputEnabled",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                return field?.GetValue(instance) is bool enabled && enabled;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting input enabled state: {ex.Message}");
                return false;
            }
        }

        private static string GetCurrentFolder(FileBrowserApp instance)
        {
            try
            {
                var field = typeof(FileBrowserApp).GetField(
                    "m_CurrentFolder",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                );
                return field?.GetValue(instance) as string;
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting current folder: {ex.Message}");
                return null;
            }
        }

        private static string GetFileNameFromButton(FileBrowserButton button)
        {
            try
            {
                if (button?.UserData?.StartsWith("###") == true)
                    return $"folder {button.FileName}";
                else
                    return button?.FileName ?? "Unknown file";
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting file name from button: {ex.Message}");
                return "Unknown file";
            }
        }
    }
}