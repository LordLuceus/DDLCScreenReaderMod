using System;
using HarmonyLib;
using RenpyLauncher;
using UnityEngine;

namespace DDLCPlusAccess
{
    [HarmonyPatch]
    public static class FileContentPatches
    {
        // Navigation state
        private static int currentLineIndex = -1;
        private static string[] fileLines = null;
        private static FileViewerApp activeFileViewer = null;
        private static string currentFileName = null;

        /// <summary>
        /// Returns true if the file viewer is active and showing text content.
        /// </summary>
        public static bool IsFileViewerActive
        {
            get
            {
                if (activeFileViewer == null || fileLines == null)
                    return false;

                // Check if the file viewer window is still active
                return activeFileViewer.Window != null && activeFileViewer.Window.activeInHierarchy;
            }
        }

        [HarmonyPatch(typeof(FileViewerApp), "OnAppStart")]
        [HarmonyPostfix]
        public static void FileViewerApp_OnAppStart_Postfix(FileViewerApp __instance)
        {
            try
            {
                // Reset state
                activeFileViewer = __instance;
                currentFileName = GetFileNameFromPath(FileBrowserApp.ViewedPath);

                // Only support line navigation for text files
                if (!IsTextFile())
                {
                    fileLines = null;
                    currentLineIndex = -1;

                    // Announce non-text file
                    string message = $"File opened: {currentFileName}. This is not a text file.";
                    ClipboardUtils.OutputGameText("", message, TextType.FileBrowser);
                    ScreenReaderMod.Logger?.Msg($"Non-text file opened: {currentFileName}");
                    return;
                }

                // Extract and split file content into lines
                string fileContent = ExtractFileContent();
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    fileLines = null;
                    currentLineIndex = -1;
                    ClipboardUtils.OutputGameText(
                        "",
                        $"File opened: {currentFileName}. File is empty.",
                        TextType.FileBrowser
                    );
                    return;
                }

                // Split content into lines, removing empty lines
                fileLines = fileContent.Split(
                    new[] { '\r', '\n' },
                    StringSplitOptions.RemoveEmptyEntries
                );

                if (fileLines.Length == 0)
                {
                    fileLines = null;
                    currentLineIndex = -1;
                    ClipboardUtils.OutputGameText(
                        "",
                        $"File opened: {currentFileName}. File is empty.",
                        TextType.FileBrowser
                    );
                    return;
                }

                // Start at the first line
                currentLineIndex = 0;

                // Announce file opened with navigation instructions
                string openMessage =
                    $"File opened: {currentFileName}. {fileLines.Length} lines. Use up and down arrows to navigate.";
                ClipboardUtils.OutputGameText("", openMessage, TextType.FileBrowser);
                ScreenReaderMod.Logger?.Msg(
                    $"File content opened: {currentFileName} ({fileLines.Length} lines)"
                );

                // Announce the first line
                AnnounceCurrentLine();
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileViewerApp_OnAppStart_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(FileViewerApp), "OnAppClose")]
        [HarmonyPostfix]
        public static void FileViewerApp_OnAppClose_Postfix(FileViewerApp __instance)
        {
            try
            {
                activeFileViewer = null;
                currentLineIndex = -1;
                fileLines = null;
                currentFileName = null;

                ScreenReaderMod.Logger?.Msg("File viewer closed");
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileViewerApp_OnAppClose_Postfix: {ex.Message}"
                );
            }
        }

        public static void NavigateToPrevious()
        {
            if (!IsFileViewerActive || fileLines == null || fileLines.Length == 0)
                return;

            // Up arrow = previous line = lower index
            if (currentLineIndex > 0)
            {
                currentLineIndex--;
                AnnounceCurrentLine();
            }
            else
            {
                ClipboardUtils.OutputGameText("", "Beginning of file", TextType.FileBrowser);
            }
        }

        public static void NavigateToNext()
        {
            if (!IsFileViewerActive || fileLines == null || fileLines.Length == 0)
                return;

            // Down arrow = next line = higher index
            if (currentLineIndex < fileLines.Length - 1)
            {
                currentLineIndex++;
                AnnounceCurrentLine();
            }
            else
            {
                ClipboardUtils.OutputGameText("", "End of file", TextType.FileBrowser);
            }
        }

        public static void NavigateToFirst()
        {
            if (!IsFileViewerActive || fileLines == null || fileLines.Length == 0)
                return;

            currentLineIndex = 0;
            AnnounceCurrentLine();
        }

        public static void NavigateToLast()
        {
            if (!IsFileViewerActive || fileLines == null || fileLines.Length == 0)
                return;

            currentLineIndex = fileLines.Length - 1;
            AnnounceCurrentLine();
        }

        private static void AnnounceCurrentLine()
        {
            if (fileLines == null || currentLineIndex < 0 || currentLineIndex >= fileLines.Length)
                return;

            string line = fileLines[currentLineIndex];
            string cleanedLine = TextHelper.CleanText(line);

            // Announce line with position info
            int lineNumber = currentLineIndex + 1;
            ScreenReaderMod.Logger?.Msg(
                $"File line {lineNumber}/{fileLines.Length}: {cleanedLine}"
            );
            ClipboardUtils.OutputGameText("", cleanedLine, TextType.FileBrowser);
        }

        private static bool IsTextFile()
        {
            try
            {
                var viewedAsset = FileBrowserApp.ViewedAsset;
                return viewedAsset is TextAsset;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error checking if file is text: {ex.Message}");
                return false;
            }
        }

        private static string ExtractFileContent()
        {
            try
            {
                var viewedAsset = FileBrowserApp.ViewedAsset;

                if (viewedAsset is TextAsset textAsset)
                {
                    string content = textAsset.text;

                    // Apply same processing as the file viewer
                    if (textAsset.name != "attributions")
                    {
                        content = Renpy.Text.GetLocalisedString(content);
                    }

                    // Convert line breaks
                    content = content.Replace("\\n", "\n");

                    return content;
                }

                return null;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error extracting file content: {ex.Message}");
                return null;
            }
        }

        private static string GetFileNameFromPath(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return "Unknown File";

                string[] pathParts = filePath.Split('/');
                string fileName = pathParts[pathParts.Length - 1];

                // Apply same localization as the viewer
                if (fileName != "attributions.txt")
                {
                    fileName = Renpy.Text.GetLocalisedString(fileName);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error getting file name from path: {ex.Message}");
                return "Unknown File";
            }
        }
    }
}
