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
            if (_isInitialized) return;

            try
            {
                GameObject managerObject = new GameObject("ScreenReaderMod_CoroutineManager");
                managerObject.AddComponent<CoroutineManager>();

                Logger.Msg("Clipboard utility initialized successfully");
                Logger.Msg("Using default settings - All text types enabled, Speaker names included");
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
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in OnUpdate: {ex.Message}");
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
