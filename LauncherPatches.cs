using System.Collections;
using HarmonyLib;
using RenpyLauncher;
using TMPro;
using UnityEngine.UI;

namespace DDLCScreenReaderMod
{
    [HarmonyPatch]
    public static class LauncherPatches
    {
        // Rate limiting for confirmation dialog to prevent spam
        private static string lastConfirmationButtonText = "";
        private static UnityEngine.GameObject lastConfirmationButtonObject = null;

        // Patch button selection for navigation announcements
        [HarmonyPatch(
            typeof(DesktopApp),
            "SelectButton",
            new System.Type[] { typeof(StartMenuButton) }
        )]
        [HarmonyPostfix]
        public static void DesktopApp_SelectButton_Postfix(DesktopApp __instance, StartMenuButton b)
        {
            try
            {
                if (b != null && b.Text != null && !string.IsNullOrWhiteSpace(b.Text.text))
                {
                    string buttonText = b.Text.text;
                    ScreenReaderMod.Logger?.Msg($"Launcher button selected: {buttonText}");
                    ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopApp_SelectButton_Postfix: {ex.Message}"
                );
            }
        }

        // Confirmation dialog accessibility
        [HarmonyPatch(typeof(DesktopConfirmationWindow), "ShowMessageDialog")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_ShowMessageDialog_Postfix(
            DesktopConfirmationWindow __instance,
            string message,
            string header
        )
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    ClipboardUtils.OutputGameText("", header, TextType.SystemMessage);
                }
                if (!string.IsNullOrWhiteSpace(message))
                {
                    ClipboardUtils.OutputGameText("", message, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_ShowMessageDialog_Postfix: {ex.Message}"
                );
            }
        }

        [HarmonyPatch(typeof(DesktopConfirmationWindow), "UpdateSelection")]
        [HarmonyPostfix]
        public static void DesktopConfirmationWindow_UpdateSelection_Postfix(
            DesktopConfirmationWindow __instance
        )
        {
            try
            {
                var currentSelected = UnityEngine
                    .EventSystems
                    .EventSystem
                    .current
                    ?.currentSelectedGameObject;
                if (currentSelected != null)
                {
                    string buttonText = "";
                    if (
                        currentSelected == __instance.ConfirmButton?.gameObject
                        && __instance.confirmText?.text != null
                    )
                    {
                        buttonText = __instance.confirmText.text;
                    }
                    else if (
                        currentSelected == __instance.CancelButton?.gameObject
                        && __instance.backText?.text != null
                    )
                    {
                        buttonText = __instance.backText.text;
                    }
                    else if (
                        currentSelected == __instance.OKButton?.gameObject
                        && __instance.okText?.text != null
                    )
                    {
                        buttonText = __instance.okText.text;
                    }

                    if (!string.IsNullOrWhiteSpace(buttonText))
                    {
                        // Only announce if this is a different button than last time
                        if (currentSelected != lastConfirmationButtonObject)
                        {
                            lastConfirmationButtonText = buttonText;
                            lastConfirmationButtonObject = currentSelected;

                            ScreenReaderMod.Logger?.Msg(
                                $"Confirmation button focused: {buttonText}"
                            );
                            ClipboardUtils.OutputGameText("", buttonText, TextType.Menu);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in DesktopConfirmationWindow_UpdateSelection_Postfix: {ex.Message}"
                );
            }
        }

        // Patch BootupApp to announce system information text
        [HarmonyPatch(typeof(BootupApp), "SetLocalization")]
        [HarmonyPostfix]
        public static void BootupApp_SetLocalization_Postfix(BootupApp __instance)
        {
            try
            {
                if (
                    __instance.checkSystemText != null
                    && !string.IsNullOrWhiteSpace(__instance.checkSystemText.text)
                )
                {
                    string systemText = __instance.checkSystemText.text;
                    ScreenReaderMod.Logger?.Msg($"Boot system info: {systemText}");
                    ClipboardUtils.OutputGameText("", systemText, TextType.SystemMessage);
                }
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error(
                    $"Error in BootupApp_SetLocalization_Postfix: {ex.Message}"
                );
            }
        }

        // Patch BootupApp sequence to announce boot messages as they appear
        [HarmonyPatch(typeof(BootupApp), "Sequence")]
        [HarmonyPostfix]
        public static void BootupApp_Sequence_Postfix(BootupApp __instance)
        {
            try
            {
                // Start monitoring the boot sequence text changes
                __instance.StartCoroutine(MonitorBootSequenceText(__instance));
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in BootupApp_Sequence_Postfix: {ex.Message}");
            }
        }

        private static IEnumerator MonitorBootSequenceText(BootupApp bootupApp)
        {
            string lastText = "";

            while (bootupApp != null && bootupApp.gameObject.activeInHierarchy)
            {
                try
                {
                    if (bootupApp.label != null && !string.IsNullOrWhiteSpace(bootupApp.label.text))
                    {
                        string currentText = bootupApp.label.text.Trim();

                        if (currentText != lastText && !string.IsNullOrEmpty(currentText))
                        {
                            // Only announce new lines, not empty text or repeated text
                            string[] lines = currentText.Split('\n');
                            string[] lastLines = lastText.Split('\n');

                            // Find new lines that weren't in the previous text
                            for (int i = lastLines.Length; i < lines.Length; i++)
                            {
                                string line = lines[i].Trim();
                                if (!string.IsNullOrEmpty(line))
                                {
                                    ScreenReaderMod.Logger?.Msg($"Boot sequence line: {line}");
                                    ClipboardUtils.OutputGameText("", line, TextType.SystemMessage);
                                }
                            }

                            lastText = currentText;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ScreenReaderMod.Logger?.Error(
                        $"Error monitoring boot sequence text: {ex.Message}"
                    );
                }

                yield return new UnityEngine.WaitForSeconds(0.1f); // Check every 100ms
            }
        }

        // Patch BiosApp to announce initial BIOS screen text
        [HarmonyPatch(typeof(BiosApp), "Sequence")]
        [HarmonyPostfix]
        public static void BiosApp_Sequence_Postfix(BiosApp __instance)
        {
            try
            {
                // Start monitoring the BIOS sequence text changes
                __instance.StartCoroutine(MonitorBiosSequenceText(__instance));
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in BiosApp_Sequence_Postfix: {ex.Message}");
            }
        }

        private static IEnumerator MonitorBiosSequenceText(BiosApp biosApp)
        {
            string lastText = "";

            while (biosApp != null && biosApp.gameObject.activeInHierarchy)
            {
                try
                {
                    if (biosApp.label != null && !string.IsNullOrWhiteSpace(biosApp.label.text))
                    {
                        string currentText = biosApp.label.text.Trim();

                        if (currentText != lastText && !string.IsNullOrEmpty(currentText))
                        {
                            // Only announce new lines, not empty text or repeated text
                            string[] lines = currentText.Split('\n');
                            string[] lastLines = lastText.Split('\n');

                            // Find new lines that weren't in the previous text
                            for (int i = lastLines.Length; i < lines.Length; i++)
                            {
                                string line = lines[i].Trim();
                                if (!string.IsNullOrEmpty(line))
                                {
                                    ScreenReaderMod.Logger?.Msg($"BIOS sequence line: {line}");
                                    ClipboardUtils.OutputGameText("", line, TextType.SystemMessage);
                                }
                            }

                            lastText = currentText;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ScreenReaderMod.Logger?.Error(
                        $"Error monitoring BIOS sequence text: {ex.Message}"
                    );
                }

                yield return new UnityEngine.WaitForSeconds(0.01f);
            }
        }

        // Patch VMApp to announce reset sequence text
        [HarmonyPatch(typeof(VMApp), "Sequence")]
        [HarmonyPostfix]
        public static void VMApp_Sequence_Postfix(VMApp __instance)
        {
            try
            {
                // Start monitoring the reset sequence text changes
                __instance.StartCoroutine(MonitorVMSequenceText(__instance));
            }
            catch (System.Exception ex)
            {
                ScreenReaderMod.Logger?.Error($"Error in VMApp_Sequence_Postfix: {ex.Message}");
            }
        }

        private static IEnumerator MonitorVMSequenceText(VMApp vmApp)
        {
            string lastText = "";

            while (vmApp != null && vmApp.gameObject.activeInHierarchy)
            {
                try
                {
                    if (vmApp.label != null && !string.IsNullOrWhiteSpace(vmApp.label.text))
                    {
                        string currentText = vmApp.label.text.Trim();

                        if (currentText != lastText && !string.IsNullOrEmpty(currentText))
                        {
                            // Only announce new lines, not empty text or repeated text
                            string[] lines = currentText.Split('\n');
                            string[] lastLines = lastText.Split('\n');

                            // Find new lines that weren't in the previous text
                            for (int i = lastLines.Length; i < lines.Length; i++)
                            {
                                string line = lines[i].Trim();
                                if (!string.IsNullOrEmpty(line))
                                {
                                    // Filter out the cursor character (█) for screen readers
                                    string cleanLine = line.Replace("█", "").Trim();
                                    if (!string.IsNullOrEmpty(cleanLine))
                                    {
                                        ScreenReaderMod.Logger?.Msg(
                                            $"Reset sequence line: {cleanLine}"
                                        );
                                        ClipboardUtils.OutputGameText(
                                            "",
                                            cleanLine,
                                            TextType.SystemMessage
                                        );
                                    }
                                }
                            }

                            lastText = currentText;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    ScreenReaderMod.Logger?.Error(
                        $"Error monitoring reset sequence text: {ex.Message}"
                    );
                }

                yield return new UnityEngine.WaitForSeconds(0.01f);
            }
        }
    }
}
