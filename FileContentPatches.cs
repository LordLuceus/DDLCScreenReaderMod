using System;
using System.Diagnostics;
using System.IO;
using HarmonyLib;
using RenpyLauncher;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class FileContentPatches
    {
        // Patch FileViewerApp to provide file content accessibility
        [HarmonyPatch(typeof(FileViewerApp), "OnAppStart")]
        [HarmonyPostfix]
        public static void FileViewerApp_OnAppStart_Postfix(FileViewerApp __instance)
        {
            try
            {

                // Extract file content
                string fileName = GetFileNameFromPath(FileBrowserApp.ViewedPath);
                string fileContent = ExtractFileContent();

                if (!string.IsNullOrWhiteSpace(fileContent))
                {
                    // Open in Notepad
                    if (TryOpenInNotepad(fileName, fileContent))
                    {
                        string notepadAnnouncement =
                            $"File opened: {fileName}. Content opened in Notepad.";
                        ClipboardUtils.OutputGameText(
                            "",
                            notepadAnnouncement,
                            TextType.FileBrowser
                        );
                    }

                    ScreenReaderMod.Logger?.Msg(
                        $"File content accessed: {fileName} ({fileContent.Length} characters)"
                    );
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in FileViewerApp_OnAppStart_Postfix: {ex.Message}"
                );
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
                else
                {
                    // Non-text file types (images, audio) - provide description
                    string fileName = GetFileNameFromPath(FileBrowserApp.ViewedPath);
                    return $"[This is a non-text file: {fileName}. Content cannot be copied to text format.]";
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error extracting file content: {ex.Message}");
                return "[Error: Unable to extract file content.]";
            }
        }

        private static bool TryOpenInNotepad(string fileName, string content)
        {
            try
            {
                // Create temporary file
                string tempDir = Path.GetTempPath();
                string tempFileName =
                    $"DDLC_{SanitizeFileName(fileName)}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string tempFilePath = Path.Combine(tempDir, tempFileName);

                // Write content to temporary file
                File.WriteAllText(tempFilePath, content, System.Text.Encoding.UTF8);

                // Open in Notepad
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "notepad.exe",
                    Arguments = $"\"{tempFilePath}\"",
                    UseShellExecute = true,
                };

                Process.Start(startInfo);

                ScreenReaderMod.Logger?.Msg($"Opened file in Notepad: {tempFilePath}");

                return true;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error opening file in Notepad: {ex.Message}");
                return false;
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

        private static string SanitizeFileName(string fileName)
        {
            try
            {
                // Remove invalid file name characters
                char[] invalidChars = Path.GetInvalidFileNameChars();
                foreach (char c in invalidChars)
                {
                    fileName = fileName.Replace(c, '_');
                }

                // Remove extension if present and add .txt
                fileName = Path.GetFileNameWithoutExtension(fileName);

                // Limit length
                if (fileName.Length > 50)
                    fileName = fileName.Substring(0, 50);

                return fileName;
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error sanitizing file name: {ex.Message}");
                return "DDLCFile";
            }
        }
    }
}
