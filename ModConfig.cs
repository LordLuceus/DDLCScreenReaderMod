using System;
using System.IO;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace DDLCScreenReaderMod
{
    public class ModConfig
    {
        private static ModConfig _instance;
        private static readonly string ConfigPath = Path.Combine(
            MelonEnvironment.UserDataDirectory,
            "DDLCScreenReaderMod",
            "config.json"
        );

        public static ModConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    LoadConfig();
                }
                return _instance;
            }
        }

        [JsonProperty("enableDialogue")]
        public bool EnableDialogue { get; set; } = true;

        [JsonProperty("enableMenus")]
        public bool EnableMenus { get; set; } = true;

        [JsonProperty("enableChoices")]
        public bool EnableChoices { get; set; } = true;

        [JsonProperty("enableSystemMessages")]
        public bool EnableSystemMessages { get; set; } = true;

        [JsonProperty("enableNarrator")]
        public bool EnableNarrator { get; set; } = true;

        [JsonProperty("includeSpeakerNames")]
        public bool IncludeSpeakerNames { get; set; } = true;

        [JsonProperty("clipboardUpdateDelay")]
        public int ClipboardUpdateDelayMs { get; set; } = 100;

        [JsonProperty("enableLogging")]
        public bool EnableLogging { get; set; } = true;

        [JsonProperty("enableVerboseLogging")]
        public bool EnableVerboseLogging { get; set; } = false;

        [JsonProperty("filterDuplicateText")]
        public bool FilterDuplicateText { get; set; } = true;

        [JsonProperty("maxTextLength")]
        public int MaxTextLength { get; set; } = 1000;

        [JsonProperty("repeatDialogueHotkey")]
        public KeyCode RepeatDialogueHotkey { get; set; } = KeyCode.R;

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    _instance = JsonConvert.DeserializeObject<ModConfig>(json) ?? new ModConfig();
                    ScreenReaderMod.Logger?.Msg("Configuration loaded successfully");
                }
                else
                {
                    _instance = new ModConfig();
                    SaveConfig();
                    ScreenReaderMod.Logger?.Msg("Default configuration created");
                }
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Failed to load configuration: {ex.Message}");
                _instance = new ModConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                string configDir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                string json = JsonConvert.SerializeObject(_instance, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                ScreenReaderMod.Logger?.Msg("Configuration saved successfully");
            }
            catch (UnauthorizedAccessException)
            {
                ScreenReaderMod.Logger?.Warning(
                    "Unable to save configuration: Access denied. Using default settings."
                );
            }
            catch (DirectoryNotFoundException)
            {
                ScreenReaderMod.Logger?.Warning(
                    "Unable to save configuration: Directory not found. Using default settings."
                );
            }
            catch (Exception ex)
            {
                ScreenReaderMod.Logger?.Warning(
                    $"Unable to save configuration: {ex.Message}. Using default settings."
                );
            }
        }

        public bool ShouldOutputTextType(TextType textType)
        {
            switch (textType)
            {
                case TextType.Dialogue:
                    return EnableDialogue;
                case TextType.MenuChoice:
                    return EnableChoices;
                case TextType.Menu:
                    return EnableMenus;
                case TextType.SystemMessage:
                    return EnableSystemMessages;
                case TextType.Narrator:
                    return EnableNarrator;
                default:
                    return true;
            }
        }

        public void ReloadConfig()
        {
            LoadConfig();
        }
    }
}
