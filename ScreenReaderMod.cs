using System;
using System.Text.RegularExpressions;
using HarmonyLib;
using MelonAccessibilityLib;
using MelonLoader;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    public class ScreenReaderMod : MelonMod
    {
        public static MelonLogger.Instance Logger { get; private set; }
        public static ScreenReaderMod Instance { get; private set; }
        private static bool _isInitialized = false;

        public override void OnInitializeMelon()
        {
            Instance = this;
            Logger = LoggerInstance;
            Logger.Msg("DDLC Screen Reader Mod initialized!");
            Logger.Msg("Waiting for Unity to be ready before starting speech system...");
        }

        private void InitializeSpeechSystem()
        {
            if (_isInitialized)
                return;

            try
            {
                // Set up accessibility library logging
                AccessibilityLog.Logger = new MelonLoggerAdapter(LoggerInstance);

                // Configure text type names for logging
                SpeechManager.TextTypeNames = GameTextType.GetTextTypeNames();

                // Configure which text types should be stored for repeat
                SpeechManager.ShouldStoreForRepeatPredicate = GameTextType.ShouldStoreForRepeat;

                // Register Ren'Py/TMP-specific text cleaning patterns
                RegisterTextCleanerPatterns();

                // Initialize the speech system
                if (SpeechManager.Initialize())
                {
                    Logger.Msg("Speech system initialized successfully");
                    Logger.Msg(
                        "Using default settings - All text types enabled, Speaker names included"
                    );
                }
                else
                {
                    Logger.Warning("Speech system initialization returned false");
                }

                // Initialize special poem descriptions
                SpecialPoemDescriptions.LoadDescriptions();
                Logger.Msg(
                    $"Special poem descriptions loaded: {SpecialPoemDescriptions.DescriptionCount} entries"
                );

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to initialize speech system: {ex.Message}");
            }
        }

        private void RegisterTextCleanerPatterns()
        {
            // Ren'Py curly brace tags: {w}, {nw}, {clear}, {fast}, {cps=...}, etc.
            TextCleaner.AddRegexReplacement(@"\{[^}]*\}", "");

            // TMP square bracket tags
            TextCleaner.AddRegexReplacement(
                @"\[color=[^\]]*\]|\[/color\]",
                "",
                RegexOptions.IgnoreCase
            );
            TextCleaner.AddRegexReplacement(
                @"\[size=[^\]]*\]|\[/size\]",
                "",
                RegexOptions.IgnoreCase
            );
            TextCleaner.AddRegexReplacement(@"\[b\]|\[/b\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[i\]|\[/i\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[u\]|\[/u\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[s\]|\[/s\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[center\]|\[/center\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[right\]|\[/right\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(@"\[left\]|\[/left\]", "", RegexOptions.IgnoreCase);
            TextCleaner.AddRegexReplacement(
                @"\[font=[^\]]*\]|\[/font\]",
                "",
                RegexOptions.IgnoreCase
            );
            TextCleaner.AddRegexReplacement(
                @"\[alpha=[^\]]*\]|\[/alpha\]",
                "",
                RegexOptions.IgnoreCase
            );

            // Special replacements
            TextCleaner.AddReplacement(
                @"<sprite name=""keyboard_enter"">Apply",
                "Press Enter to Apply"
            );
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Logger.Msg($"Scene loaded: {sceneName} (Index: {buildIndex})");

            // Initialize speech system on first scene load (when Unity is ready)
            InitializeSpeechSystem();

            // Announce scene changes
            if (sceneName == "LauncherScene")
            {
                ClipboardUtils.OutputGameText("", "DDLC Plus launcher", TextType.Menu);
            }
        }

        public override void OnUpdate()
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    ClipboardUtils.RepeatCurrentDialogue();
                }

                if (Input.GetKeyDown(KeyCode.C) && IsInSettingsApp())
                {
                    AnnounceDataCollectionPercentage();
                }

                if (Input.GetKeyDown(KeyCode.P) && IsInJukeboxApp())
                {
                    AnnounceJukeboxPosition();
                }

                // History screen navigation
                if (HistoryPatches.IsHistoryScreenActive)
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        HistoryPatches.NavigateToPrevious();
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        HistoryPatches.NavigateToNext();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnUpdate: {ex.Message}");
            }
        }

        private void AnnounceDataCollectionPercentage()
        {
            try
            {
                if (UnlockSystem.Instance != null)
                {
                    float percentage = UnlockSystem.Instance.UnlockedPercentage;
                    int roundedPercentage = Mathf.RoundToInt(percentage);
                    string message = $"Data collected: {roundedPercentage} percent";

                    Logger?.Msg($"Data collection percentage requested: {message}");
                    ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                }
                else
                {
                    Logger?.Warning(
                        "UnlockSystem.Instance is null - cannot retrieve data collection percentage"
                    );
                    ClipboardUtils.OutputGameText(
                        "",
                        "Data collection percentage unavailable",
                        TextType.SystemMessage
                    );
                }
            }
            catch (Exception ex)
            {
                Logger?.Error($"Error announcing data collection percentage: {ex.Message}");
                ClipboardUtils.OutputGameText(
                    "",
                    "Error retrieving data collection percentage",
                    TextType.SystemMessage
                );
            }
        }

        private bool IsInSettingsApp()
        {
            try
            {
                var settingsApp = UnityEngine.Object.FindObjectOfType<RenpyLauncher.SettingsApp>();
                return settingsApp != null && settingsApp.gameObject.activeInHierarchy;
            }
            catch (System.Exception ex)
            {
                Logger?.Error($"Error checking if in settings app: {ex.Message}");
                return false;
            }
        }

        private bool IsInJukeboxApp()
        {
            try
            {
                var jukeboxApp = UnityEngine.Object.FindObjectOfType<JukeboxApp>();
                if (jukeboxApp == null)
                    return false;

                // Check if the jukebox app is actually open and active
                // The Window component should be active when the app is open
                return jukeboxApp.gameObject.activeInHierarchy
                    && jukeboxApp.Window != null
                    && jukeboxApp.Window.activeInHierarchy;
            }
            catch (System.Exception ex)
            {
                Logger?.Error($"Error checking if in jukebox app: {ex.Message}");
                return false;
            }
        }

        private void AnnounceJukeboxPosition()
        {
            try
            {
                var jukeboxApp = UnityEngine.Object.FindObjectOfType<JukeboxApp>();
                if (jukeboxApp?.JukeboxPlayer == null)
                {
                    ClipboardUtils.OutputGameText("", "No jukebox player active", TextType.Jukebox);
                    return;
                }

                var currentTrack = jukeboxApp.JukeboxPlayer.GetCurrentTrack();
                if (!currentTrack.CurrentTrackValid())
                {
                    ClipboardUtils.OutputGameText("", "No track playing", TextType.Jukebox);
                    return;
                }

                var trackInfo = jukeboxApp.JukeboxPlayer.GetCurrentTrackEntry();
                if (trackInfo == null)
                {
                    ClipboardUtils.OutputGameText(
                        "",
                        "No track information available",
                        TextType.Jukebox
                    );
                    return;
                }

                var playerState = jukeboxApp.JukeboxPlayer.GetPlayerState();
                if (playerState == JukeboxPlayer.PlayerState.Stopped)
                {
                    ClipboardUtils.OutputGameText(
                        "",
                        $"Stopped on {trackInfo.TrackName}",
                        TextType.Jukebox
                    );
                    return;
                }

                float currentPosition = jukeboxApp.JukeboxPlayer.GetCurrentTrackProgress();
                float totalLength = trackInfo.Length;

                // Convert to minutes:seconds format
                int currentMinutes = (int)Math.Floor(currentPosition / 60f);
                int currentSeconds = (int)Math.Floor(currentPosition - (float)currentMinutes * 60f);
                int totalMinutes = (int)Math.Floor(totalLength / 60f);
                int totalSeconds = (int)Math.Floor(totalLength - (float)totalMinutes * 60f);

                string positionText = $"{currentMinutes}:{currentSeconds:D2}";
                string totalText = $"{totalMinutes}:{totalSeconds:D2}";

                string message = $"{trackInfo.TrackName}, {positionText} of {totalText}";

                Logger?.Msg($"Jukebox position requested: {message}");
                ClipboardUtils.OutputGameText("", message, TextType.Jukebox);
            }
            catch (Exception ex)
            {
                Logger?.Error($"Error announcing jukebox position: {ex.Message}");
                ClipboardUtils.OutputGameText(
                    "",
                    "Error retrieving playback position",
                    TextType.Jukebox
                );
            }
        }

        public override void OnDeinitializeMelon()
        {
            SpeechManager.Stop();
            Logger.Msg("Screen Reader Mod deinitialized.");
        }

        public override void OnApplicationQuit()
        {
            Logger.Msg("DDLC Application quitting - Screen Reader Mod cleanup");
        }
    }
}
