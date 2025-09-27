using System;
using HarmonyLib;
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
            Logger.Msg("Waiting for Unity to be ready before starting clipboard system...");
        }

        private void InitializeClipboardSystem()
        {
            if (_isInitialized)
                return;

            try
            {
                GameObject managerObject = new GameObject("ScreenReaderMod_CoroutineManager");
                managerObject.AddComponent<CoroutineManager>();

                Logger.Msg("Clipboard utility initialized successfully");
                Logger.Msg(
                    "Using default settings - All text types enabled, Speaker names included"
                );

                // Initialize special poem descriptions
                SpecialPoemDescriptions.LoadDescriptions();
                Logger.Msg(
                    $"Special poem descriptions loaded: {SpecialPoemDescriptions.DescriptionCount} entries"
                );

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to initialize clipboard system: {ex.Message}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Logger.Msg($"Scene loaded: {sceneName} (Index: {buildIndex})");

            // Initialize clipboard system on first scene load (when Unity is ready)
            InitializeClipboardSystem();

            // Test clipboard functionality and announce scene changes
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
            if (CoroutineManager.Instance != null)
            {
                CoroutineManager.Instance.StopClipboardProcessor();
                UnityEngine.Object.Destroy(CoroutineManager.Instance.gameObject);
            }

            Logger.Msg("Screen Reader Mod deinitialized.");
        }

        public override void OnApplicationQuit()
        {
            Logger.Msg("DDLC Application quitting - Screen Reader Mod cleanup");
        }
    }
}
