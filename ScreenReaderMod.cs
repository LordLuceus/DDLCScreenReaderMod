using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    public class ScreenReaderMod : MelonMod
    {
        public static MelonLogger.Instance Logger { get; private set; }

        public override void OnInitializeMelon()
        {
            Logger = LoggerInstance;
            Logger.Msg("DDLC Screen Reader Mod initialized!");

            try
            {
                ModConfig.LoadConfig();
                ClipboardUtils.Initialize();
                Logger.Msg("Clipboard utility initialized successfully");
                Logger.Msg(
                    $"Configuration loaded - Dialogue: {ModConfig.Instance.EnableDialogue}, Menus: {ModConfig.Instance.EnableMenus}, Choices: {ModConfig.Instance.EnableChoices}, Speaker Names: {ModConfig.Instance.IncludeSpeakerNames}"
                );
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to initialize mod: {ex.Message}");
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Logger.Msg($"Scene loaded: {sceneName} (Index: {buildIndex})");

            // Test clipboard functionality and announce scene changes
            if (sceneName == "LauncherScene")
            {
                ClipboardUtils.OutputGameText("", "DDLC Plus launcher", TextType.Menu);
            }
        }

        public override void OnApplicationQuit()
        {
            Logger.Msg("DDLC Application quitting - Screen Reader Mod cleanup");
            ClipboardUtils.Cleanup();
        }
    }
}
